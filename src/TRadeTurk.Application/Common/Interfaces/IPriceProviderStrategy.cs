namespace TRadeTurk.Application.Common.Interfaces;

public interface IPriceProviderStrategy
{
    string ProviderName { get; }
    Task<decimal> GetCurrentPriceAsync(string symbol, CancellationToken cancellationToken = default);
}
