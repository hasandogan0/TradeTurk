using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TRadeTurk.Domain.Interfaces;

namespace TRadeTurk.Infrastructure.Services;

/// <summary>
/// Proxy Pattern: Binance API çağrılarını hız sınırlarına takılmamak ve performansı artırmak için Cache'leyen yapı.
/// </summary>
public class BinanceProxyService : IBinanceService
{
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
        string cacheKey = $"Binance_Price_{symbol}";

        // Proxy Cache Kontrolü
        if (_cache.TryGetValue(cacheKey, out decimal cachedPrice))
        {
            _logger.LogInformation("Price for {Symbol} retrieved from cache (PROXY used).", symbol);
            return cachedPrice;
        }

        _logger.LogInformation("Cache miss for {Symbol}. Fetching real-time price from Binance API...", symbol);
        
        // Gerçek servisten veriyi çek
        decimal realTimePrice = await _realBinanceService.GetCurrentPriceAsync(symbol, cancellationToken);

        if (realTimePrice > 0)
        {
            // Hız Sınırı ve Limitlerini aşmamak için 1 dakikalık önbellek süresi (Gerçek veri olduğu için süreyi kısalttık)
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(30));
                
            _cache.Set(cacheKey, realTimePrice, cacheEntryOptions);
        }

        return realTimePrice;
    }
}
