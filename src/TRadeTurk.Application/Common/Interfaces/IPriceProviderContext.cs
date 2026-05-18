namespace TRadeTurk.Application.Common.Interfaces;

public interface IPriceProviderContext
{
    Task<decimal> GetCurrentPriceAsync(string symbol, CancellationToken cancellationToken = default);
}
