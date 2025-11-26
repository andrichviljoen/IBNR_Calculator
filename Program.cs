using IBNRCalculator.Calculations;
using IBNRCalculator.Models;
using IBNRCalculator.Services;

var app = new App();
await app.RunAsync(args);

internal record CliOptions(
    string? DatabasePath,
    string TableName,
    bool UseSample,
    OriginType OriginType,
    OriginGrain OriginGrain,
    int DevelopmentMonths,
    IReadOnlyCollection<int>? AllowedSteps,
    string AccidentColumn,
    string UnderwritingColumn,
    string PaymentColumn,
    string AmountColumn,
    string? DateFormat);

internal class App
{
    public async Task RunAsync(string[] args)
    {
        if (args.Length == 0 || args.Contains("--help"))
        {
            PrintHelp();
            return;
        }

        var options = ParseArguments(args);
        ITransactionSource source = options.UseSample
            ? new SampleDataSource()
            : new AccessDataSource(
                options.DatabasePath!,
                options.TableName,
                options.AccidentColumn,
                options.UnderwritingColumn,
                options.PaymentColumn,
                options.AmountColumn,
                options.DateFormat);

        var transactions = await source.LoadAsync();
        var triangle = TriangleBuilder.Build(transactions, options.OriginType, options.OriginGrain, options.DevelopmentMonths);
        var result = ChainLadderCalculator.Calculate(triangle, options.AllowedSteps);

        Console.WriteLine($"Loaded {transactions.Count} transactions. Origin={options.OriginType}, Grain={options.OriginGrain}, Development step={options.DevelopmentMonths} months.");
        Console.WriteLine();

        PrintTriangle("Incremental Triangle", triangle.Incremental);
        PrintTriangle("Cumulative Triangle", triangle.Cumulative);
        PrintLinkRatios(result.LinkRatios, options.DevelopmentMonths);
        PrintOriginSummaries(result.OriginSummaries);
    }

    private static CliOptions ParseArguments(string[] args)
    {
        string? databasePath = null;
        string tableName = "Claims";
        var originType = OriginType.Accident;
        var originGrain = OriginGrain.Year;
        int developmentMonths = 12;
        bool useSample = false;
        List<int>? allowedSteps = null;
        string accidentColumn = "AccidentDate";
        string underwritingColumn = "UnderwritingDate";
        string paymentColumn = "PaymentDate";
        string amountColumn = "IncrementalPaid";
        string? dateFormat = null;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--database":
                    databasePath = args[++i];
                    break;
                case "--table":
                    tableName = args[++i];
                    break;
                case "--origin":
                    originType = ParseOriginType(args[++i]);
                    break;
                case "--origin-grain":
                    originGrain = ParseOriginGrain(args[++i]);
                    break;
                case "--development-months":
                    developmentMonths = int.Parse(args[++i]);
                    break;
                case "--use-steps":
                    allowedSteps = args[++i]
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Select(int.Parse)
                        .ToList();
                    break;
                case "--sample":
                    useSample = true;
                    break;
                case "--accident-column":
                    accidentColumn = args[++i];
                    break;
                case "--underwriting-column":
                    underwritingColumn = args[++i];
                    break;
                case "--payment-column":
                    paymentColumn = args[++i];
                    break;
                case "--amount-column":
                    amountColumn = args[++i];
                    break;
                case "--date-format":
                    dateFormat = args[++i];
                    break;
                default:
                    throw new ArgumentException($"Unknown argument {args[i]}");
            }
        }

        if (!useSample && string.IsNullOrWhiteSpace(databasePath))
        {
            throw new ArgumentException("--database is required unless --sample is passed");
        }

        return new CliOptions(databasePath, tableName, useSample, originType, originGrain, developmentMonths, allowedSteps, accidentColumn, underwritingColumn, paymentColumn, amountColumn, dateFormat);
    }

    private static OriginType ParseOriginType(string value) => value.ToLowerInvariant() switch
    {
        "accident" => OriginType.Accident,
        "underwriting" => OriginType.Underwriting,
        _ => throw new ArgumentException($"Unknown origin type {value}")
    };

    private static OriginGrain ParseOriginGrain(string value) => value.ToLowerInvariant() switch
    {
        "year" => OriginGrain.Year,
        "quarter" => OriginGrain.Quarter,
        _ => throw new ArgumentException($"Unknown origin grain {value}")
    };

    private static void PrintTriangle(string title, IReadOnlyDictionary<string, SortedDictionary<int, decimal>> triangle)
    {
        Console.WriteLine(title);
        Console.WriteLine(new string('-', title.Length));
        var orderedOrigins = triangle.Keys.OrderBy(k => k).ToList();
        var maxDevelopment = triangle.Values.SelectMany(v => v.Keys).DefaultIfEmpty(0).Max();
        Console.Write("Origin\\Dev");
        for (var dev = 0; dev <= maxDevelopment; dev++)
        {
            Console.Write($"\t{dev}");
        }
        Console.WriteLine();

        foreach (var origin in orderedOrigins)
        {
            Console.Write(origin);
            for (var dev = 0; dev <= maxDevelopment; dev++)
            {
                triangle[origin].TryGetValue(dev, out var value);
                Console.Write($"\t{value:N0}");
            }
            Console.WriteLine();
        }

        Console.WriteLine();
    }

    private static void PrintLinkRatios(IEnumerable<LinkRatioStep> linkRatios, int stepMonths)
    {
        Console.WriteLine("Selected link ratios");
        Console.WriteLine(new string('-', 23));
        Console.WriteLine($"Step ({stepMonths}-month grain)\tFactor\tObservations");
        foreach (var ratio in linkRatios)
        {
            Console.WriteLine($"{ratio.FromDevelopment}->{ratio.ToDevelopment}\t{ratio.Factor:F3}\t{ratio.ObservationCount}");
        }
        Console.WriteLine();
    }

    private static void PrintOriginSummaries(IEnumerable<OriginSummary> origins)
    {
        Console.WriteLine("Origin ultimates and IBNR");
        Console.WriteLine(new string('-', 26));
        Console.WriteLine("Origin\tLatest Cumulative\tUltimate\tIBNR");
        foreach (var origin in origins)
        {
            Console.WriteLine($"{origin.OriginKey}\t{origin.LatestCumulative:N0}\t{origin.Ultimate:N0}\t{origin.IncurredButNotReported:N0}");
        }
    }

    private static void PrintHelp()
    {
        Console.WriteLine("IBNR Calculator (chain ladder)");
        Console.WriteLine("Usage: dotnet run -- --database <path> --table <table> [options]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --database <path>           Path to the Access .mdb/.accdb file");
        Console.WriteLine("  --table <name>              Table containing transaction data (default: Claims)");
        Console.WriteLine("  --origin <accident|underwriting>   Origin basis (default: accident)");
        Console.WriteLine("  --origin-grain <year|quarter>      Calendar grouping for origins (default: year)");
        Console.WriteLine("  --development-months <n>    Number of months per development period (default: 12)");
        Console.WriteLine("  --use-steps <list>          Comma-separated development step indices to include (e.g. 0,1,2)");
        Console.WriteLine("  --accident-column <name>    Override column name for accident date (default: AccidentDate)");
        Console.WriteLine("  --underwriting-column <name>Override column name for underwriting date (default: UnderwritingDate)");
        Console.WriteLine("  --payment-column <name>     Override column name for payment date (default: PaymentDate)");
        Console.WriteLine("  --amount-column <name>      Override column name for incremental paid amount (default: IncrementalPaid)");
        Console.WriteLine("  --date-format <format>      Parse Access date columns using the supplied format (e.g. yyyyMMdd or yyyyQQ)");
        Console.WriteLine("  --sample                    Use bundled synthetic data instead of an Access database");
        Console.WriteLine("  --help                      Show this help");
    }
}
