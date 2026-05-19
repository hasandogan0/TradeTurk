using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRadeTurk.Application.Common.Interfaces;
using TRadeTurk.Application.Features.Transactions.Queries;

namespace TRadeTurk.WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/transactions")]
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserContext _currentUserContext;

    public TransactionsController(IMediator mediator, ICurrentUserContext currentUserContext)
    {
        _mediator = mediator;
        _currentUserContext = currentUserContext;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMine()
    {
        var userId = User.SetCurrentUserFromClaims(_currentUserContext);
        var transactions = await _mediator.Send(new GetMyTransactionsQuery { UserId = userId });
        return Ok(transactions);
    }
}
