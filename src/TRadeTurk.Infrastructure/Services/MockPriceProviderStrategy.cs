using TRadeTurk.Application.Common.Interfaces;

namespace TRadeTurk.Infrastructure.Services;

public class MockPriceProviderStrategy : IPriceProviderStrategy
{
    private static readonly IReadOnlyDictionary<string, decimal> Prices = new Dictionary<string, decimal>
    {
        ["BTCUSDT"] = 65000m,
        ["ETHUSDT"] = 3200m,
        ["BNBUSDT"] = 580m
    };

    public string ProviderName => "Mock";

    public Task<decimal> GetCurrentPriceAsync(string symbol, CancellationToken cancellationToken = default)
    {
        var normalizedSymbol = symbol.Trim().ToUpperInvariant();
        var basePrice = Prices.GetValueOrDefault(normalizedSymbol, 100m);
        var variation = (decimal)(Random.Shared.NextDouble() - 0.5) * basePrice * 0.01m;

        return Task.FromResult(decimal.Round(basePrice + variation, 4));
    }
}
