namespace IBNRCalculator.Models;

public class Triangle
{
    public Triangle(
        Dictionary<string, SortedDictionary<int, decimal>> incremental,
        int developmentStepMonths,
        OriginGrain originGrain)
    {
        Incremental = incremental;
        DevelopmentStepMonths = developmentStepMonths;
        OriginGrain = originGrain;
        DevelopmentPeriods = incremental.Values.SelectMany(v => v.Keys).DefaultIfEmpty(0).Max() + 1;
    }

    public IReadOnlyDictionary<string, SortedDictionary<int, decimal>> Incremental { get; }

    public int DevelopmentStepMonths { get; }

    public int DevelopmentPeriods { get; }

    public OriginGrain OriginGrain { get; }

    public IReadOnlyDictionary<string, SortedDictionary<int, decimal>> Cumulative =>
        Incremental.ToDictionary(
            kvp => kvp.Key,
            kvp => new SortedDictionary<int, decimal>(
                kvp.Value.OrderBy(cell => cell.Key)
                    .Aggregate(new SortedDictionary<int, decimal>(), (dict, cell) =>
                    {
                        var previous = dict.Count == 0 ? 0m : dict.Values.Last();
                        dict[cell.Key] = previous + cell.Value;
                        return dict;
                    })));

    public IEnumerable<string> OrderedOrigins =>
        Incremental.Keys.OrderBy(k => k);
}
