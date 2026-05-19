using MediatR;
using TRadeTurk.Application.Common;

namespace TRadeTurk.Application.Features.Markets.Queries;

public class GetMarketSymbolsQuery : IRequest<IReadOnlyCollection<string>>
{
}

public class GetMarketSymbolsQueryHandler : IRequestHandler<GetMarketSymbolsQuery, IReadOnlyCollection<string>>
{
    public Task<IReadOnlyCollection<string>> Handle(GetMarketSymbolsQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(MarketSymbols.Supported);
    }
}
