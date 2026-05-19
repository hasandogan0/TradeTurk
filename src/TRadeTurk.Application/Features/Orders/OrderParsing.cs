using TRadeTurk.Domain.Enums;

namespace TRadeTurk.Application.Features.Orders;

internal static class OrderParsing
{
    public static OrderSide ParseSide(string value)
    {
        return value.Trim().ToUpperInvariant() switch
        {
            "BUY" => OrderSide.Buy,
            "SELL" => OrderSide.Sell,
            _ => throw new InvalidOperationException("Order side BUY veya SELL olmalidir.")
        };
    }

    public static OrderType ParseType(string value)
    {
        return value.Trim().ToUpperInvariant().Replace("-", "_") switch
        {
            "MARKET" => OrderType.Market,
            "LIMIT" => OrderType.Limit,
            "STOP_LOSS" => OrderType.StopLoss,
            "TAKE_PROFIT" => OrderType.TakeProfit,
            _ => throw new InvalidOperationException("Order type MARKET, LIMIT, STOP_LOSS veya TAKE_PROFIT olmalidir.")
        };
    }
}
