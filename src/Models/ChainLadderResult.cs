namespace IBNRCalculator.Models;

public record LinkRatioStep(int FromDevelopment, int ToDevelopment, decimal Factor, int ObservationCount);

public record OriginSummary(string OriginKey, decimal LatestCumulative, decimal Ultimate, decimal IncurredButNotReported);

public class ChainLadderResult
{
    public ChainLadderResult(Triangle triangle, IEnumerable<LinkRatioStep> linkRatios, IEnumerable<OriginSummary> origins)
    {
        Triangle = triangle;
        LinkRatios = linkRatios.ToList();
        OriginSummaries = origins.ToList();
    }

    public Triangle Triangle { get; }

    public IReadOnlyList<LinkRatioStep> LinkRatios { get; }

    public IReadOnlyList<OriginSummary> OriginSummaries { get; }
}
