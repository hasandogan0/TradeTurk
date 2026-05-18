namespace TRadeTurk.Application.DTOs;

public class WalletDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public decimal FiatBalance { get; set; }
}
