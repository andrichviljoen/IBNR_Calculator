using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using IBNRCalculator.Calculations;
using IBNRCalculator.Models;
using IBNRCalculator.Services;

namespace IBNRCalculator.ViewModels;

public class ReservingViewModel : INotifyPropertyChanged
{
    private string? _databasePath;
    private string _tableName = "Claims";
    private bool _useSample = true;
    private OriginType _originType = OriginType.Accident;
    private OriginGrain _originGrain = OriginGrain.Year;
    private int _developmentMonths = 12;
    private string _accidentColumn = "AccidentDate";
    private string _underwritingColumn = "UnderwritingDate";
    private string _paymentColumn = "PaymentDate";
    private string _amountColumn = "IncrementalPaid";
    private string? _dateFormat;
    private string? _allowedSteps;
    private bool _isBusy;
    private string? _status;
    private ObservableCollection<LinkRatioStep> _linkRatios = new();
    private ObservableCollection<OriginSummary> _originSummaries = new();

    public ReservingViewModel()
    {
        RunCommand = new AsyncCommand(RunAsync, () => !IsBusy);
    }

    public string? DatabasePath
    {
        get => _databasePath;
        set => SetProperty(ref _databasePath, value);
    }

    public string TableName
    {
        get => _tableName;
        set => SetProperty(ref _tableName, value);
    }

    public bool UseSample
    {
        get => _useSample;
        set => SetProperty(ref _useSample, value);
    }

    public OriginType OriginType
    {
        get => _originType;
        set => SetProperty(ref _originType, value);
    }

    public OriginGrain OriginGrain
    {
        get => _originGrain;
        set => SetProperty(ref _originGrain, value);
    }

    public int DevelopmentMonths
    {
        get => _developmentMonths;
        set => SetProperty(ref _developmentMonths, value);
    }

    public string AccidentColumn
    {
        get => _accidentColumn;
        set => SetProperty(ref _accidentColumn, value);
    }

    public string UnderwritingColumn
    {
        get => _underwritingColumn;
        set => SetProperty(ref _underwritingColumn, value);
    }

    public string PaymentColumn
    {
        get => _paymentColumn;
        set => SetProperty(ref _paymentColumn, value);
    }

    public string AmountColumn
    {
        get => _amountColumn;
        set => SetProperty(ref _amountColumn, value);
    }

    public string? DateFormat
    {
        get => _dateFormat;
        set => SetProperty(ref _dateFormat, value);
    }

    public string? AllowedSteps
    {
        get => _allowedSteps;
        set => SetProperty(ref _allowedSteps, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                (RunCommand as AsyncCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public string? Status
    {
        get => _status;
        private set => SetProperty(ref _status, value);
    }

    public ObservableCollection<LinkRatioStep> LinkRatios
    {
        get => _linkRatios;
        private set => SetProperty(ref _linkRatios, value);
    }

    public ObservableCollection<OriginSummary> OriginSummaries
    {
        get => _originSummaries;
        private set => SetProperty(ref _originSummaries, value);
    }

    public ICommand RunCommand { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    private async Task RunAsync()
    {
        try
        {
            IsBusy = true;
            Status = "Loading transactions...";

            ITransactionSource source = UseSample
                ? new SampleDataSource()
                : new AccessDataSource(
                    DatabasePath ?? throw new InvalidOperationException("Database path is required."),
                    TableName,
                    AccidentColumn,
                    UnderwritingColumn,
                    PaymentColumn,
                    AmountColumn,
                    DateFormat);

            var transactions = await source.LoadAsync();
            Status = "Building triangle...";

            var triangle = TriangleBuilder.Build(transactions, OriginType, OriginGrain, DevelopmentMonths);
            var allowedSteps = ParseAllowedSteps();
            var result = ChainLadderCalculator.Calculate(triangle, allowedSteps);

            LinkRatios = new ObservableCollection<LinkRatioStep>(result.LinkRatios);
            OriginSummaries = new ObservableCollection<OriginSummary>(result.OriginSummaries);
            Status = $"Loaded {transactions.Count} records. {OriginSummaries.Count} origin periods calculated.";
        }
        catch (Exception ex)
        {
            Status = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private List<int>? ParseAllowedSteps()
    {
        if (string.IsNullOrWhiteSpace(AllowedSteps))
        {
            return null;
        }

        var steps = AllowedSteps
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(int.Parse)
            .ToList();

        return steps.Count == 0 ? null : steps;
    }

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}

internal class AsyncCommand : ICommand
{
    private readonly Func<Task> _execute;
    private readonly Func<bool>? _canExecute;

    public AsyncCommand(Func<Task> execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

    public async void Execute(object? parameter) => await _execute();

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
