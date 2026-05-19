using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRadeTurk.Application.Common.Interfaces;
using TRadeTurk.Application.Features.Orders.Commands;
using TRadeTurk.Application.Features.Orders.Queries;

namespace TRadeTurk.WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserContext _currentUserContext;

    public OrdersController(IMediator mediator, ICurrentUserContext currentUserContext)
    {
        _mediator = mediator;
        _currentUserContext = currentUserContext;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderCommand command)
    {
        User.SetCurrentUserFromClaims(_currentUserContext);
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("open")]
    public async Task<IActionResult> Open()
    {
        User.SetCurrentUserFromClaims(_currentUserContext);
        return Ok(await _mediator.Send(new GetOpenOrdersQuery()));
    }

    [HttpGet("history")]
    public async Task<IActionResult> History()
    {
        User.SetCurrentUserFromClaims(_currentUserContext);
        return Ok(await _mediator.Send(new GetOrderHistoryQuery()));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        User.SetCurrentUserFromClaims(_currentUserContext);
        var order = await _mediator.Send(new GetOrderByIdQuery { Id = id });
        return order == null ? NotFound(new { message = "Emir bulunamadi." }) : Ok(order);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        User.SetCurrentUserFromClaims(_currentUserContext);
        var result = await _mediator.Send(new CancelOrderCommand { Id = id });
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
