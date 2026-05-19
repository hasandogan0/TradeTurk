using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRadeTurk.Application.Common.Interfaces;
using TRadeTurk.Application.Features.Wallets.Queries;

namespace TRadeTurk.WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/wallet")]
public class WalletController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserContext _currentUserContext;

    public WalletController(IMediator mediator, ICurrentUserContext currentUserContext)
    {
        _mediator = mediator;
        _currentUserContext = currentUserContext;
    }

    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetWallet(Guid userId)
    {
        _currentUserContext.SetUserId(userId);
        var wallet = await _mediator.Send(new GetWalletQuery { UserId = userId });

        return wallet == null ? NotFound(new { message = "Cuzdan bulunamadi." }) : Ok(wallet);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyWallet()
    {
        var userId = User.SetCurrentUserFromClaims(_currentUserContext);
        var wallet = await _mediator.Send(new GetMyWalletQuery { UserId = userId });

        return wallet == null ? NotFound(new { message = "Cuzdan bulunamadi." }) : Ok(wallet);
    }
}
