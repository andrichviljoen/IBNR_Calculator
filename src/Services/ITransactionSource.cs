using IBNRCalculator.Models;

namespace IBNRCalculator.Services;

public interface ITransactionSource
{
    Task<IReadOnlyCollection<ClaimTransaction>> LoadAsync();
}
