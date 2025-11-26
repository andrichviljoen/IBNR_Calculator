using IBNRCalculator.Models;

namespace IBNRCalculator.Calculations;

public static class PeriodBuilder
{
    public static DateTime GetOriginPeriodStart(DateTime originDate, OriginGrain grain)
    {
        var year = originDate.Year;
        if (grain == OriginGrain.Year)
        {
            return new DateTime(year, 1, 1);
        }

        var quarter = ((originDate.Month - 1) / 3) + 1;
        var month = ((quarter - 1) * 3) + 1;
        return new DateTime(year, month, 1);
    }

    public static string GetOriginKey(DateTime originDate, OriginGrain grain)
    {
        var start = GetOriginPeriodStart(originDate, grain);
        if (grain == OriginGrain.Year)
        {
            return start.ToString("yyyy");
        }

        var quarter = ((start.Month - 1) / 3) + 1;
        return $"{start:yyyy}-Q{quarter}";
    }

    public static int GetDevelopmentPeriodIndex(DateTime originStart, DateTime paymentDate, int developmentStepMonths)
    {
        var months = ((paymentDate.Year - originStart.Year) * 12) + (paymentDate.Month - originStart.Month);
        if (months < 0)
        {
            return 0;
        }

        return months / developmentStepMonths;
    }
}
