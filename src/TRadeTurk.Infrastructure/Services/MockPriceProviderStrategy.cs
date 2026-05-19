using TRadeTurk.Application.Common.Interfaces;

namespace TRadeTurk.Infrastructure.Services;

public class MockPriceProviderStrategy : IPriceProviderStrategy
{
    private static readonly IReadOnlyDictionary<string, decimal> Prices = new Dictionary<string, decimal>
    {
        ["BTCUSDT"] = 65000m,
        ["ETHUSDT"] = 3200m,
        ["BNBUSDT"] = 580m,
        ["SOLUSDT"] = 150m,
        ["XRPUSDT"] = 0.62m,
        ["ADAUSDT"] = 0.42m,
        ["DOGEUSDT"] = 0.14m,
        ["AVAXUSDT"] = 32m,
        ["DOTUSDT"] = 6.4m,
        ["LINKUSDT"] = 14m,
        ["MATICUSDT"] = 0.75m,
        ["LTCUSDT"] = 84m,
        ["TRXUSDT"] = 0.12m,
        ["ATOMUSDT"] = 8.8m,
        ["NEARUSDT"] = 6.1m
    };

    public string ProviderName => "Mock";

    public Task<decimal> GetCurrentPriceAsync(string symbol, CancellationToken cancellationToken = default)
    {
        var normalizedSymbol = symbol.Trim().ToUpperInvariant();
        return Task.FromResult(Prices.GetValueOrDefault(normalizedSymbol, 100m));
    }
}
