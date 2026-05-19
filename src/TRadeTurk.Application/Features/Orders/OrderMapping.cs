using TRadeTurk.Application.DTOs;
using TRadeTurk.Domain.Entities;

namespace TRadeTurk.Application.Features.Orders;

internal static class OrderMapping
{
    public static OrderDto ToDto(Order order) => new()
    {
        Id = order.Id,
        Symbol = order.Symbol,
        Side = order.Side.ToString().ToUpperInvariant(),
        Type = order.Type.ToString().Replace("StopLoss", "STOP_LOSS").Replace("TakeProfit", "TAKE_PROFIT").ToUpperInvariant(),
        Status = order.Status.ToString().Replace("PartiallyFilled", "PARTIALLY_FILLED").ToUpperInvariant(),
        Quantity = order.Quantity,
        Price = order.Price,
        TriggerPrice = order.TriggerPrice,
        FilledQuantity = order.FilledQuantity,
        AverageFillPrice = order.AverageFillPrice,
        Total = order.Total,
        CreatedAt = order.CreatedAt,
        FilledAt = order.FilledAt,
        CancelledAt = order.CancelledAt
    };
}
