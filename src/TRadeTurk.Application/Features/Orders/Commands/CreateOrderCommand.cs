using MediatR;
using TRadeTurk.Application.DTOs;

namespace TRadeTurk.Application.Features.Orders.Commands;

public class CreateOrderCommand : IRequest<OrderResultDto>
{
    public string Symbol { get; set; } = string.Empty;
    public string Side { get; set; } = "BUY";
    public string Type { get; set; } = "MARKET";
    public decimal Quantity { get; set; }
    public decimal? Price { get; set; }
    public decimal? TriggerPrice { get; set; }
}
