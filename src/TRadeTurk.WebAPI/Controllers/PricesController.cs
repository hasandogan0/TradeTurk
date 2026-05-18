using MediatR;
using Microsoft.AspNetCore.Mvc;
using TRadeTurk.Application.Features.Prices.Queries;

namespace TRadeTurk.WebAPI.Controllers;

[ApiController]
[Route("api/prices")]
public class PricesController : ControllerBase
{
    private readonly IMediator _mediator;

    public PricesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{symbol}")]
    public async Task<IActionResult> GetPrice(string symbol)
    {
        var price = await _mediator.Send(new GetLivePriceQuery { Symbol = symbol });

        return Ok(price);
    }
}
