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
        string amountColumn = "IncrementalPaid",
        string? dateFormat = null)
    {
        DatabasePath = databasePath;
        TableName = tableName;
        AccidentColumn = accidentColumn;
        UnderwritingColumn = underwritingColumn;
        PaymentColumn = paymentColumn;
        AmountColumn = amountColumn;
        DateFormat = dateFormat;
    }

    public string DatabasePath { get; }
    public string TableName { get; }
    public string AccidentColumn { get; }
    public string UnderwritingColumn { get; }
    public string PaymentColumn { get; }
    public string AmountColumn { get; }
    public string? DateFormat { get; }

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
            var accidentDate = ParseDate(reader[0]);
            var underwritingDate = ParseDate(reader[1]);
            var paymentDate = ParseDate(reader[2]);
            var incrementalPaid = reader.GetDecimal(3);
            results.Add(new ClaimTransaction(accidentDate, underwritingDate, paymentDate, incrementalPaid));
        }

        return results;
    }

    private DateTime ParseDate(object value)
    {
        if (value is DateTime dt)
        {
            return dt.Date;
        }

        if (DateFormat is not null)
        {
            var text = Convert.ToString(value)?.Trim();
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new FormatException("Encountered a blank date value when a format was supplied.");
            }

            if (DateFormat.Equals("yyyyQQ", StringComparison.OrdinalIgnoreCase) && text!.Length == 6)
            {
                var year = int.Parse(text.Substring(0, 4));
                var quarter = int.Parse(text.Substring(4, 2));
                if (quarter is < 1 or > 4)
                {
                    throw new FormatException($"Quarter value '{quarter}' must be between 1 and 4.");
                }

                var month = (quarter - 1) * 3 + 1;
                return new DateTime(year, month, 1);
            }

            return DateTime.ParseExact(text!, DateFormat, null);
        }

        return Convert.ToDateTime(value);
    }
}
