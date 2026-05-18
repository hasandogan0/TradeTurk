using MediatR;
using TRadeTurk.Application.DTOs;

namespace TRadeTurk.Application.Features.Prices.Queries;

public class GetLivePriceQuery : IRequest<PriceDto>
{
    public string Symbol { get; set; } = string.Empty;
}
