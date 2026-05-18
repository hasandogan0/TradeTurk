namespace TRadeTurk.Application.DTOs;

public class PriceDto
{
    public string Symbol { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime RetrievedAtUtc { get; set; }
}
