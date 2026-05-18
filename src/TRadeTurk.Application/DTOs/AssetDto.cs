namespace TRadeTurk.Application.DTOs;

public class AssetDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid WalletId { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal AverageCost { get; set; }
}
