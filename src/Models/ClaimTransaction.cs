namespace IBNRCalculator.Models;

public record ClaimTransaction(
    DateTime AccidentDate,
    DateTime UnderwritingDate,
    DateTime PaymentDate,
    decimal IncrementalPaid)
{
    public DateTime GetOriginDate(OriginType originType) => originType switch
    {
        OriginType.Accident => AccidentDate,
        OriginType.Underwriting => UnderwritingDate,
        _ => throw new ArgumentOutOfRangeException(nameof(originType), originType, "Unsupported origin type")
    };
}
