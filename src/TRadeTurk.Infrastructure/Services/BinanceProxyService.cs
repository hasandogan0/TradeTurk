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
    // Gerçek servis (HttpClient vb.) inject edilebilir, simülasyon nedeniyle mocklanmıştır.

    public BinanceProxyService(IMemoryCache cache, ILogger<BinanceProxyService> logger)
    {
        _cache = cache;
        _logger = logger;
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

        _logger.LogInformation("Fetching real-time price from 'Binance API' for {Symbol}...", symbol);
        
        // Simüle edilmiş API isteği bekleme süresi
        await Task.Delay(500, cancellationToken); 
        decimal realTimePrice = GenerateSimulatedPrice(symbol);

        // Hız Sınırı ve Limitlerini aşmamak için 2 dakikalık önbellek süresi
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(2));
            
        _cache.Set(cacheKey, realTimePrice, cacheEntryOptions);

        return realTimePrice;
    }
    
    // Yardımcı: Sadece testler / simülasyon için kurgulanmış piyasa fiyatları
    private decimal GenerateSimulatedPrice(string symbol)
    {
        return symbol switch 
        {
            "BTCUSDT" => 68500.50m + (decimal)(new Random().NextDouble() * 10 - 5), // Rastgele volatilite simülasyonu
            "ETHUSDT" => 3500.25m + (decimal)(new Random().NextDouble() * 5 - 2.5),
            _ => 100.0m
        };
    }
}
