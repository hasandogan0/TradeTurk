using MediatR;
using TRadeTurk.Application.Common;
using TRadeTurk.Application.Common.Interfaces;
using TRadeTurk.Application.DTOs;

namespace TRadeTurk.Application.Features.Markets.Queries;

public class GetMarketTickersQuery : IRequest<IReadOnlyCollection<MarketTickerDto>>
{
}

public class GetMarketTickersQueryHandler : IRequestHandler<GetMarketTickersQuery, IReadOnlyCollection<MarketTickerDto>>
{
    private readonly IMarketDataService _marketDataService;

    public GetMarketTickersQueryHandler(IMarketDataService marketDataService)
    {
        _marketDataService = marketDataService;
    }

    public Task<IReadOnlyCollection<MarketTickerDto>> Handle(GetMarketTickersQuery request, CancellationToken cancellationToken)
    {
        return _marketDataService.GetTickersAsync(MarketSymbols.Supported, cancellationToken);
    }
}
