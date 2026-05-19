using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRadeTurk.Application.Common.Interfaces;
using TRadeTurk.Application.Features.Assets.Commands;

namespace TRadeTurk.WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/trade")]
public class TradeController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserContext _currentUserContext;

    public TradeController(IMediator mediator, ICurrentUserContext currentUserContext)
    {
        _mediator = mediator;
        _currentUserContext = currentUserContext;
    }

    [HttpPost("buy")]
    public async Task<IActionResult> BuyAsset([FromBody] BuyAssetCommand command)
    {
        User.SetCurrentUserFromClaims(_currentUserContext);
        var result = await _mediator.Send(command);

        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("sell")]
    public async Task<IActionResult> SellAsset([FromBody] SellAssetCommand command)
    {
        User.SetCurrentUserFromClaims(_currentUserContext);
        var result = await _mediator.Send(command);

        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
