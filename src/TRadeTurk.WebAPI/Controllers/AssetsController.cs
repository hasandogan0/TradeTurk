using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRadeTurk.Application.Common.Interfaces;
using TRadeTurk.Application.Features.Assets.Queries;

namespace TRadeTurk.WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/assets")]
public class AssetsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserContext _currentUserContext;

    public AssetsController(IMediator mediator, ICurrentUserContext currentUserContext)
    {
        _mediator = mediator;
        _currentUserContext = currentUserContext;
    }

    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetAssets(Guid userId)
    {
        _currentUserContext.SetUserId(userId);
        var assets = await _mediator.Send(new GetUserAssetsQuery { UserId = userId });

        return Ok(assets);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyAssets()
    {
        var userId = User.SetCurrentUserFromClaims(_currentUserContext);
        var assets = await _mediator.Send(new GetUserAssetsQuery { UserId = userId });

        return Ok(assets);
    }
}
