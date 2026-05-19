using MediatR;
using TRadeTurk.Application.DTOs;

namespace TRadeTurk.Application.Features.Portfolio.Queries;

public class GetPortfolioSummaryQuery : IRequest<PortfolioSummaryDto?>
{
    public Guid UserId { get; set; }
}
