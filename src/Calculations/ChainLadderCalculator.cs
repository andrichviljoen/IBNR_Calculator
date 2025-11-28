using IBNRCalculator.Models;

namespace IBNRCalculator.Calculations;

public static class ChainLadderCalculator
{
    public static ChainLadderResult Calculate(Triangle triangle, IEnumerable<int>? allowedSteps = null)
    {
        var allowedSet = allowedSteps?.ToHashSet() ?? Enumerable.Range(0, triangle.DevelopmentPeriods - 1).ToHashSet();
        var cumulative = triangle.Cumulative;

        var linkRatios = new List<LinkRatioStep>();
        var lastDevelopmentIndex = triangle.DevelopmentPeriods - 1;

        for (var step = 0; step < lastDevelopmentIndex; step++)
        {
            if (!allowedSet.Contains(step))
            {
                linkRatios.Add(new LinkRatioStep(step, step + 1, 1m, 0));
                continue;
            }

            decimal numerator = 0;
            decimal denominator = 0;
            var observationCount = 0;
            foreach (var origin in cumulative.Values)
            {
                if (origin.TryGetValue(step, out var current) && origin.TryGetValue(step + 1, out var next) && current > 0)
                {
                    numerator += next;
                    denominator += current;
                    observationCount++;
                }
            }

            var factor = denominator == 0 ? 1m : numerator / denominator;
            linkRatios.Add(new LinkRatioStep(step, step + 1, factor, observationCount));
        }

        var originSummaries = new List<OriginSummary>();
        foreach (var originKey in triangle.OrderedOrigins)
        {
            var developmentByOrigin = cumulative[originKey];
            var latestKnown = developmentByOrigin.OrderBy(kv => kv.Key).Last();
            var latestDevIndex = latestKnown.Key;
            var ultimate = latestKnown.Value;

            for (var step = latestDevIndex; step < linkRatios.Count; step++)
            {
                ultimate *= linkRatios[step].Factor;
            }

            var ibnr = ultimate - latestKnown.Value;
            originSummaries.Add(new OriginSummary(originKey, latestKnown.Value, ultimate, ibnr));
        }

        return new ChainLadderResult(triangle, linkRatios, originSummaries);
    }
}
