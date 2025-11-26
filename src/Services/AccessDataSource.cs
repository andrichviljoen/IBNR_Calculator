using System.Data.OleDb;
using IBNRCalculator.Models;

namespace IBNRCalculator.Services;

public class AccessDataSource : ITransactionSource
{
    public AccessDataSource(
        string databasePath,
        string tableName,
        string accidentColumn = "AccidentDate",
        string underwritingColumn = "UnderwritingDate",
        string paymentColumn = "PaymentDate",
        string amountColumn = "IncrementalPaid")
    {
        DatabasePath = databasePath;
        TableName = tableName;
        AccidentColumn = accidentColumn;
        UnderwritingColumn = underwritingColumn;
        PaymentColumn = paymentColumn;
        AmountColumn = amountColumn;
    }

    public string DatabasePath { get; }
    public string TableName { get; }
    public string AccidentColumn { get; }
    public string UnderwritingColumn { get; }
    public string PaymentColumn { get; }
    public string AmountColumn { get; }

    public async Task<IReadOnlyCollection<ClaimTransaction>> LoadAsync()
    {
        var connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={DatabasePath};Persist Security Info=False;";
        using var connection = new OleDbConnection(connectionString);
        await connection.OpenAsync();

        var commandText = $"SELECT [{AccidentColumn}], [{UnderwritingColumn}], [{PaymentColumn}], [{AmountColumn}] FROM [{TableName}]";
        using var command = new OleDbCommand(commandText, connection);
        using var reader = await command.ExecuteReaderAsync();
        var results = new List<ClaimTransaction>();

        while (reader != null && await reader.ReadAsync())
        {
            var accidentDate = reader.GetDateTime(0);
            var underwritingDate = reader.GetDateTime(1);
            var paymentDate = reader.GetDateTime(2);
            var incrementalPaid = reader.GetDecimal(3);
            results.Add(new ClaimTransaction(accidentDate, underwritingDate, paymentDate, incrementalPaid));
        }

        return results;
    }
}
