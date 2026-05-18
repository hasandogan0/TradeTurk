namespace TRadeTurk.Application.Common.Interfaces;

public interface IBinancePriceService
{
    Task<decimal> GetCurrentPriceAsync(string symbol, CancellationToken cancellationToken = default);
}
