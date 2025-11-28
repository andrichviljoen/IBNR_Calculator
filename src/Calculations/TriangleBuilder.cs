using IBNRCalculator.Models;

namespace IBNRCalculator.Calculations;

public static class TriangleBuilder
{
    public static Triangle Build(
        IEnumerable<ClaimTransaction> transactions,
        OriginType originType,
        OriginGrain originGrain,
        int developmentStepMonths)
    {
        var incremental = new Dictionary<string, SortedDictionary<int, decimal>>();

        foreach (var transaction in transactions)
        {
            var originDate = transaction.GetOriginDate(originType);
            var originStart = PeriodBuilder.GetOriginPeriodStart(originDate, originGrain);
            var originKey = PeriodBuilder.GetOriginKey(originDate, originGrain);
            var developmentIndex = PeriodBuilder.GetDevelopmentPeriodIndex(originStart, transaction.PaymentDate, developmentStepMonths);

            if (!incremental.TryGetValue(originKey, out var developmentCells))
            {
                developmentCells = new SortedDictionary<int, decimal>();
                incremental[originKey] = developmentCells;
            }

            developmentCells[developmentIndex] = developmentCells.GetValueOrDefault(developmentIndex) + transaction.IncrementalPaid;
        }

        var maxDevelopment = incremental.Values.SelectMany(v => v.Keys).DefaultIfEmpty(0).Max();
        foreach (var cell in incremental.Values)
        {
            for (var i = 0; i <= maxDevelopment; i++)
            {
                if (!cell.ContainsKey(i))
                {
                    cell[i] = 0;
                }
            }
        }

        return new Triangle(incremental, developmentStepMonths, originGrain);
    }
}
