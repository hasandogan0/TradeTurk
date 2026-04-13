using TRadeTurk.Domain.Common;

namespace TRadeTurk.Domain.Entities;

public class Wallet : BaseEntity
{
    public decimal FiatBalance { get; private set; } // TRY or USDT balance
    
    // Navigation properties
    public ICollection<Asset> Assets { get; private set; } = new List<Asset>();
    public ICollection<Transaction> Transactions { get; private set; } = new List<Transaction>();
    public Card? VirtualCard { get; private set; }

    public Wallet()
    {
        FiatBalance = 0;
    }

    public void AddFiat(decimal amount)
    {
        if(amount <= 0) throw new ArgumentException("Deposit amount must be positive.");
        FiatBalance += amount;
    }

    public void DeductFiat(decimal amount)
    {
        if(amount <= 0) throw new ArgumentException("Deduct amount must be positive.");
        if(FiatBalance < amount) throw new InvalidOperationException("Yetersiz bakiye.");
        FiatBalance -= amount;
    }
}
