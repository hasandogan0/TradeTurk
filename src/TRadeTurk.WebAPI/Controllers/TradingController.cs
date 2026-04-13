using MediatR;
using Microsoft.AspNetCore.Mvc;
using TRadeTurk.Application.Features.Assets.Commands;

namespace TRadeTurk.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TradingController : ControllerBase
{
    private readonly IMediator _mediator;

    public TradingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("buy")]
    public async Task<IActionResult> BuyAsset([FromBody] BuyAssetCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }

    [HttpPost("sell")]
    public async Task<IActionResult> SellAsset([FromBody] SellAssetCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }
}
