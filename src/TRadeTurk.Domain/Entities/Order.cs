using TRadeTurk.Domain.Common;
using TRadeTurk.Domain.Enums;

namespace TRadeTurk.Domain.Entities;

public class Order : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Symbol { get; private set; } = string.Empty;
    public OrderSide Side { get; private set; }
    public OrderType Type { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal? Price { get; private set; }
    public decimal? TriggerPrice { get; private set; }
    public decimal FilledQuantity { get; private set; }
    public decimal? AverageFillPrice { get; private set; }
    public decimal Total { get; private set; }
    public DateTime? FilledAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }

    private Order()
    {
    }

    public Order(Guid userId, string symbol, OrderSide side, OrderType type, decimal quantity, decimal? price, decimal? triggerPrice)
    {
        if (userId == Guid.Empty) throw new ArgumentException("UserId must be valid.");
        if (string.IsNullOrWhiteSpace(symbol)) throw new ArgumentException("Symbol cannot be empty.");
        if (quantity <= 0) throw new ArgumentException("Quantity must be greater than zero.");
        if ((type == OrderType.Limit || type == OrderType.Market) && price is <= 0) throw new ArgumentException("Price must be greater than zero.");
        if ((type == OrderType.StopLoss || type == OrderType.TakeProfit) && triggerPrice is <= 0) throw new ArgumentException("Trigger price must be greater than zero.");

        UserId = userId;
        Symbol = symbol.Trim().ToUpperInvariant();
        Side = side;
        Type = type;
        Status = type == OrderType.Market ? OrderStatus.Filled : OrderStatus.Pending;
        Quantity = quantity;
        Price = price;
        TriggerPrice = triggerPrice;
    }

    public void MarkPending()
    {
        Status = OrderStatus.Pending;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Fill(decimal executedPrice)
    {
        if (executedPrice <= 0) throw new ArgumentException("Executed price must be greater than zero.");

        FilledQuantity = Quantity;
        AverageFillPrice = executedPrice;
        Total = Quantity * executedPrice;
        Status = OrderStatus.Filled;
        FilledAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status != OrderStatus.Pending) throw new InvalidOperationException("Sadece bekleyen emirler iptal edilebilir.");

        Status = OrderStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Fail()
    {
        Status = OrderStatus.Failed;
        UpdatedAt = DateTime.UtcNow;
    }
}
