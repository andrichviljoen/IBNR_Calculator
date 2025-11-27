using System.Collections.Generic;
using IBNRCalculator.Models;

namespace IBNRCalculator.ViewModels;

public static class OriginTypes
{
    public static IReadOnlyList<OriginType> All { get; } = new[]
    {
        OriginType.Accident,
        OriginType.Underwriting
    };
}

public static class OriginGrains
{
    public static IReadOnlyList<OriginGrain> All { get; } = new[]
    {
        OriginGrain.Year,
        OriginGrain.Quarter
    };
}
