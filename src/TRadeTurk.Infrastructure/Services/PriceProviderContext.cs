using Microsoft.Extensions.Configuration;
using TRadeTurk.Application.Common.Interfaces;

namespace TRadeTurk.Infrastructure.Services;

public class PriceProviderContext : IPriceProviderContext
{
    private readonly IEnumerable<IPriceProviderStrategy> _strategies;
    private readonly IConfiguration _configuration;

    public PriceProviderContext(IEnumerable<IPriceProviderStrategy> strategies, IConfiguration configuration)
    {
        _strategies = strategies;
        _configuration = configuration;
    }

    public Task<decimal> GetCurrentPriceAsync(string symbol, CancellationToken cancellationToken = default)
    {
        var activeProvider = _configuration["PriceProvider:Active"] ?? "Binance";
        var strategy = _strategies.FirstOrDefault(s => string.Equals(s.ProviderName, activeProvider, StringComparison.OrdinalIgnoreCase))
            ?? _strategies.First(s => s.ProviderName == "Binance");

        return strategy.GetCurrentPriceAsync(symbol, cancellationToken);
    }
}
