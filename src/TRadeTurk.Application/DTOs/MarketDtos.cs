namespace TRadeTurk.Application.DTOs;

public class MarketTickerDto
{
    public string Symbol { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal ChangePercent24h { get; set; }
    public decimal High24h { get; set; }
    public decimal Low24h { get; set; }
    public decimal Volume24h { get; set; }
    public DateTime RetrievedAtUtc { get; set; } = DateTime.UtcNow;
}
