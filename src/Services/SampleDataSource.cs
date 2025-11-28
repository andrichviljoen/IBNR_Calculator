using IBNRCalculator.Models;

namespace IBNRCalculator.Services;

public class SampleDataSource : ITransactionSource
{
    public Task<IReadOnlyCollection<ClaimTransaction>> LoadAsync()
    {
        var baseDate = new DateTime(2018, 1, 1);
        var transactions = new List<ClaimTransaction>
        {
            new(baseDate, baseDate, baseDate.AddMonths(6), 120_000m),
            new(baseDate, baseDate, baseDate.AddMonths(12), 80_000m),
            new(baseDate, baseDate, baseDate.AddMonths(18), 60_000m),
            new(baseDate.AddYears(1), baseDate.AddYears(1), baseDate.AddYears(1).AddMonths(6), 130_000m),
            new(baseDate.AddYears(1), baseDate.AddYears(1), baseDate.AddYears(1).AddMonths(12), 70_000m),
            new(baseDate.AddYears(2), baseDate.AddYears(2), baseDate.AddYears(2).AddMonths(6), 90_000m),
            new(baseDate.AddYears(2), baseDate.AddYears(2), baseDate.AddYears(2).AddMonths(12), 110_000m),
            new(baseDate.AddYears(2), baseDate.AddYears(2), baseDate.AddYears(2).AddMonths(18), 60_000m),
            new(baseDate.AddYears(3), baseDate.AddYears(3), baseDate.AddYears(3).AddMonths(6), 75_000m),
            new(baseDate.AddYears(3), baseDate.AddYears(3), baseDate.AddYears(3).AddMonths(12), 55_000m),
            new(baseDate.AddYears(4), baseDate.AddYears(4), baseDate.AddYears(4).AddMonths(6), 40_000m),
        };

        return Task.FromResult<IReadOnlyCollection<ClaimTransaction>>(transactions);
    }
}
