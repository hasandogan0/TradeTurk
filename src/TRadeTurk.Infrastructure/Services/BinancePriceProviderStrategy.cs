using TRadeTurk.Application.Common.Interfaces;

namespace TRadeTurk.Infrastructure.Services;

public class BinancePriceProviderStrategy : IPriceProviderStrategy
{
    private readonly IBinancePriceService _binancePriceService;

    public BinancePriceProviderStrategy(IBinancePriceService binancePriceService)
    {
        _binancePriceService = binancePriceService;
    }

    public string ProviderName => "Binance";

    public Task<decimal> GetCurrentPriceAsync(string symbol, CancellationToken cancellationToken = default)
    {
        return _binancePriceService.GetCurrentPriceAsync(symbol, cancellationToken);
    }
}
