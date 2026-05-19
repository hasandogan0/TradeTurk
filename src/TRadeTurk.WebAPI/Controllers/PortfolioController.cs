using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRadeTurk.Application.Common.Interfaces;
using TRadeTurk.Application.Features.Portfolio.Queries;

namespace TRadeTurk.WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/portfolio")]
public class PortfolioController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserContext _currentUserContext;

    public PortfolioController(IMediator mediator, ICurrentUserContext currentUserContext)
    {
        _mediator = mediator;
        _currentUserContext = currentUserContext;
    }

    [HttpGet("summary/me")]
    public async Task<IActionResult> GetSummary()
    {
        var userId = User.SetCurrentUserFromClaims(_currentUserContext);
        var summary = await _mediator.Send(new GetPortfolioSummaryQuery { UserId = userId });
        return summary == null ? NotFound(new { message = "Portfoy bulunamadi." }) : Ok(summary);
    }
}
