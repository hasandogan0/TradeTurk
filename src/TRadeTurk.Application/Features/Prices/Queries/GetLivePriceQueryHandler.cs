using MediatR;
using TRadeTurk.Application.Common.Interfaces;
using TRadeTurk.Application.DTOs;

namespace TRadeTurk.Application.Features.Prices.Queries;

public class GetLivePriceQueryHandler : IRequestHandler<GetLivePriceQuery, PriceDto>
{
    private readonly IPriceProviderContext _priceProviderContext;

    public GetLivePriceQueryHandler(IPriceProviderContext priceProviderContext)
    {
        _priceProviderContext = priceProviderContext;
    }

    public async Task<PriceDto> Handle(GetLivePriceQuery request, CancellationToken cancellationToken)
    {
        var symbol = request.Symbol.Trim().ToUpperInvariant();
        var price = await _priceProviderContext.GetCurrentPriceAsync(symbol, cancellationToken);

        return new PriceDto
        {
            Symbol = symbol,
            Price = price,
            RetrievedAtUtc = DateTime.UtcNow
        };
    }
}
