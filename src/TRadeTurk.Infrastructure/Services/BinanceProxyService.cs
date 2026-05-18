using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TRadeTurk.Application.Common.Interfaces;

namespace TRadeTurk.Infrastructure.Services;

/// <summary>
/// Proxy Pattern: Binance API calls are cached, rate limited and protected with fallback data.
/// </summary>
public class BinanceProxyService : IBinancePriceService
{
    private static readonly SemaphoreSlim RateLimitGate = new(1, 1);

    private readonly IMemoryCache _cache;
    private readonly ILogger<BinanceProxyService> _logger;
    private readonly BinanceService _realBinanceService;

    public BinanceProxyService(IMemoryCache cache, ILogger<BinanceProxyService> logger, BinanceService realBinanceService)
    {
        _cache = cache;
        _logger = logger;
        _realBinanceService = realBinanceService;
    }

    public async Task<decimal> GetCurrentPriceAsync(string symbol, CancellationToken cancellationToken = default)
    {
        var normalizedSymbol = symbol.Trim().ToUpperInvariant();
        var cacheKey = $"Binance_Price_{normalizedSymbol}";
        var fallbackKey = $"Binance_LastGoodPrice_{normalizedSymbol}";

        if (_cache.TryGetValue(cacheKey, out decimal cachedPrice))
        {
            _logger.LogInformation("Price for {Symbol} retrieved from proxy cache.", normalizedSymbol);
            return cachedPrice;
        }

        await RateLimitGate.WaitAsync(cancellationToken);
        try
        {
            if (_cache.TryGetValue(cacheKey, out cachedPrice))
            {
                return cachedPrice;
            }

            var realTimePrice = await _realBinanceService.GetCurrentPriceAsync(normalizedSymbol, cancellationToken);

            if (realTimePrice > 0)
            {
                _cache.Set(cacheKey, realTimePrice, TimeSpan.FromSeconds(30));
                _cache.Set(fallbackKey, realTimePrice, TimeSpan.FromMinutes(10));
            }

            return realTimePrice;
        }
        catch (Exception ex)
        {
            if (_cache.TryGetValue(fallbackKey, out decimal fallbackPrice))
            {
                _logger.LogWarning(ex, "Binance failed for {Symbol}. Returning fallback price.", normalizedSymbol);
                return fallbackPrice;
            }

            throw;
        }
        finally
        {
            _ = Task.Delay(250, CancellationToken.None)
                .ContinueWith(_ => RateLimitGate.Release(), CancellationToken.None);
        }
    }
}
