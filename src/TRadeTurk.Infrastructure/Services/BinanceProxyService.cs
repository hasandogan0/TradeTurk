using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TRadeTurk.Application.Common;
using TRadeTurk.Application.Common.Interfaces;
using TRadeTurk.Application.DTOs;

namespace TRadeTurk.Infrastructure.Services;

/// <summary>
/// Proxy Pattern: Binance API calls are cached, rate limited and protected with fallback data.
/// </summary>
public class BinanceProxyService : IBinancePriceService, IMarketDataService
{
    private static readonly SemaphoreSlim RateLimitGate = new(1, 1);
    private static readonly IReadOnlyDictionary<string, decimal> DemoFallbackPrices = new Dictionary<string, decimal>
    {
        ["BTCUSDT"] = 106500m,
        ["ETHUSDT"] = 3950m,
        ["BNBUSDT"] = 650m,
        ["SOLUSDT"] = 178m,
        ["XRPUSDT"] = 2.25m,
        ["ADAUSDT"] = 0.72m,
        ["DOGEUSDT"] = 0.18m,
        ["AVAXUSDT"] = 39m,
        ["DOTUSDT"] = 7.3m,
        ["LINKUSDT"] = 18.5m,
        ["MATICUSDT"] = 0.92m,
        ["LTCUSDT"] = 92m,
        ["TRXUSDT"] = 0.29m,
        ["ATOMUSDT"] = 8.1m,
        ["NEARUSDT"] = 6.4m
    };

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

            if (DemoFallbackPrices.TryGetValue(normalizedSymbol, out var demoPrice))
            {
                _logger.LogWarning(ex, "Binance failed for {Symbol}. Returning demo fallback price.", normalizedSymbol);
                return demoPrice;
            }

            throw;
        }
        finally
        {
            _ = Task.Delay(250, CancellationToken.None)
                .ContinueWith(_ => RateLimitGate.Release(), CancellationToken.None);
        }
    }

    public async Task<MarketTickerDto> GetTickerAsync(string symbol, CancellationToken cancellationToken = default)
    {
        var normalizedSymbol = symbol.Trim().ToUpperInvariant();
        var cacheKey = $"Binance_Ticker_{normalizedSymbol}";

        if (_cache.TryGetValue(cacheKey, out MarketTickerDto? cachedTicker) && cachedTicker != null)
        {
            return cachedTicker;
        }

        try
        {
            var ticker = await _realBinanceService.GetTickerAsync(normalizedSymbol, cancellationToken);
            if (ticker.Price > 0)
            {
                _cache.Set(cacheKey, ticker, TimeSpan.FromSeconds(20));
                _cache.Set($"Binance_LastGoodTicker_{normalizedSymbol}", ticker, TimeSpan.FromMinutes(10));
            }

            return ticker;
        }
        catch (Exception ex)
        {
            if (_cache.TryGetValue($"Binance_LastGoodTicker_{normalizedSymbol}", out MarketTickerDto? fallbackTicker) && fallbackTicker != null)
            {
                _logger.LogWarning(ex, "Binance ticker failed for {Symbol}. Returning fallback ticker.", normalizedSymbol);
                return fallbackTicker;
            }

            if (DemoFallbackPrices.ContainsKey(normalizedSymbol))
            {
                _logger.LogWarning(ex, "Binance ticker failed for {Symbol}. Returning demo fallback ticker.", normalizedSymbol);
                return CreateDemoTicker(normalizedSymbol);
            }

            throw;
        }
    }

    public async Task<IReadOnlyCollection<MarketTickerDto>> GetTickersAsync(IReadOnlyCollection<string> symbols, CancellationToken cancellationToken = default)
    {
        const string cacheKey = "Binance_Tickers_Supported";
        if (_cache.TryGetValue(cacheKey, out IReadOnlyCollection<MarketTickerDto>? cachedTickers) && cachedTickers != null)
        {
            return cachedTickers;
        }

        await RateLimitGate.WaitAsync(cancellationToken);
        try
        {
            if (_cache.TryGetValue(cacheKey, out cachedTickers) && cachedTickers != null)
            {
                return cachedTickers;
            }

            var tickers = await _realBinanceService.GetTickersAsync(symbols, cancellationToken);
            var ordered = MarketSymbols.Supported
                .Select(symbol => tickers.FirstOrDefault(t => t.Symbol == symbol) ?? CreateDemoTicker(symbol))
                .ToArray();
            _cache.Set(cacheKey, ordered, TimeSpan.FromSeconds(20));
            return ordered;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Binance ticker batch failed. Returning demo fallback tickers.");
            return symbols
                .Where(MarketSymbols.IsSupported)
                .Select(CreateDemoTicker)
                .ToArray();
        }
        finally
        {
            _ = Task.Delay(250, CancellationToken.None)
                .ContinueWith(_ => RateLimitGate.Release(), CancellationToken.None);
        }
    }

    private static MarketTickerDto CreateDemoTicker(string symbol)
    {
        var price = DemoFallbackPrices.GetValueOrDefault(symbol, 0m);
        var seed = symbol.Sum(c => c);
        var change = ((seed % 900) - 350) / 100m;

        return new MarketTickerDto
        {
            Symbol = symbol,
            Price = price,
            ChangePercent24h = change,
            High24h = Math.Round(price * 1.035m, 8),
            Low24h = Math.Round(price * 0.965m, 8),
            Volume24h = 1000000m + seed * 2500m,
            RetrievedAtUtc = DateTime.UtcNow
        };
    }
}
