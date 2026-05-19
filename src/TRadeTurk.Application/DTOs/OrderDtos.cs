namespace TRadeTurk.Application.DTOs;

public class OrderDto
{
    public Guid Id { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string Side { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal? Price { get; set; }
    public decimal? TriggerPrice { get; set; }
    public decimal FilledQuantity { get; set; }
    public decimal? AverageFillPrice { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? FilledAt { get; set; }
    public DateTime? CancelledAt { get; set; }
}

public class OrderResultDto
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public OrderDto? Order { get; set; }
}
