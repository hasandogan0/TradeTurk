using MediatR;
using TRadeTurk.Application.DTOs;

namespace TRadeTurk.Application.Features.Portfolio.Queries;

public class GetPortfolioHistoryQuery : IRequest<IReadOnlyCollection<PortfolioHistoryPointDto>>
{
    public string Range { get; set; } = "7D";
}
