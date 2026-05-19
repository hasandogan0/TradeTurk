using MediatR;
using Microsoft.AspNetCore.Mvc;
using TRadeTurk.Application.Features.Markets.Queries;

namespace TRadeTurk.WebAPI.Controllers;

[ApiController]
[Route("api/markets")]
public class MarketsController : ControllerBase
{
    private readonly IMediator _mediator;

    public MarketsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("symbols")]
    public async Task<IActionResult> GetSymbols()
    {
        return Ok(await _mediator.Send(new GetMarketSymbolsQuery()));
    }

    [HttpGet("tickers")]
    public async Task<IActionResult> GetTickers()
    {
        return Ok(await _mediator.Send(new GetMarketTickersQuery()));
    }
}
