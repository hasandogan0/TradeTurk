namespace TRadeTurk.Application.DTOs;

public class PortfolioHistoryPointDto
{
    public DateTime CreatedAt { get; set; }
    public decimal TotalValue { get; set; }
    public decimal AvailableUSDT { get; set; }
    public decimal AssetValue { get; set; }
    public decimal TotalPnL { get; set; }
}
