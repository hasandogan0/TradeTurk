using TRadeTurk.Application.DTOs;

namespace TRadeTurk.Application.Common.Interfaces;

public interface IMarketDataService
{
    Task<MarketTickerDto> GetTickerAsync(string symbol, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<MarketTickerDto>> GetTickersAsync(IReadOnlyCollection<string> symbols, CancellationToken cancellationToken = default);
}
