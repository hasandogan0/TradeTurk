using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRadeTurk.Application.Common.Interfaces;
using TRadeTurk.Application.Features.Users.Commands;
using TRadeTurk.Application.Features.Users.Queries;

namespace TRadeTurk.WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserContext _currentUserContext;

    public UsersController(IMediator mediator, ICurrentUserContext currentUserContext)
    {
        _mediator = mediator;
        _currentUserContext = currentUserContext;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userId = User.SetCurrentUserFromClaims(_currentUserContext);
        var user = await _mediator.Send(new GetCurrentUserQuery { UserId = userId });
        return user == null ? NotFound(new { message = "Kullanici bulunamadi." }) : Ok(user);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateCurrentUserCommand command)
    {
        command.UserId = User.SetCurrentUserFromClaims(_currentUserContext);
        var user = await _mediator.Send(command);
        return user == null ? NotFound(new { message = "Kullanici bulunamadi." }) : Ok(user);
    }

    [HttpPut("me/password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
    {
        command.UserId = User.SetCurrentUserFromClaims(_currentUserContext);
        var changed = await _mediator.Send(command);
        return changed ? Ok(new { message = "Sifre guncellendi." }) : NotFound(new { message = "Kullanici bulunamadi." });
    }
}
