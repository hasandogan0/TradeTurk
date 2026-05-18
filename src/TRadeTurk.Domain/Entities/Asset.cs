using TRadeTurk.Domain.Common;

namespace TRadeTurk.Domain.Entities;

public class Asset : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid WalletId { get; private set; }
    public Wallet Wallet { get; private set; } = null!;

    public string Symbol { get; private set; } // e.g. "BTC", "ETH"
    public decimal Amount { get; private set; }
    public decimal AverageCost { get; private set; }

    private Asset() { // EF Core
        Symbol = string.Empty;
    }

    public Asset(Guid userId, Guid walletId, string symbol)
    {
        if (userId == Guid.Empty) throw new ArgumentException("UserId must be valid.");
        if (walletId == Guid.Empty) throw new ArgumentException("WalletId must be valid.");
        if (string.IsNullOrWhiteSpace(symbol)) throw new ArgumentException("Symbol cannot be empty.");

        UserId = userId;
        WalletId = walletId;
        Symbol = symbol.Trim().ToUpperInvariant();
        Amount = 0;
        AverageCost = 0;
    }

    public void AddAmount(decimal amount, decimal price)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be greater than zero.");
        
        var totalCost = (Amount * AverageCost) + (amount * price);
        Amount += amount;
        AverageCost = totalCost / Amount;
    }

    public void DeductAmount(decimal amount)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be greater than zero.");
        if(Amount < amount) throw new InvalidOperationException("Yetersiz kripto varlık.");
        
        Amount -= amount;
        
        if (Amount == 0) AverageCost = 0;
    }
}
