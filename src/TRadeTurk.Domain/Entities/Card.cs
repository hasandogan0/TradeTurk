using TRadeTurk.Domain.Common;

namespace TRadeTurk.Domain.Entities;

public class Card : BaseEntity
{
    public string CardHolderName { get; private set; }
    public string CardNumber { get; private set; }
    public string ExpiryDate { get; private set; }
    public string Cvv { get; private set; }
    public decimal Balance { get; private set; } // Sanal kartın limiti/bakiyesi

    public Guid WalletId { get; private set; }
    public Wallet Wallet { get; private set; } = null!;

    private Card() { 
        CardHolderName = string.Empty;
        CardNumber = string.Empty;
        ExpiryDate = string.Empty;
        Cvv = string.Empty;
    }

    public Card(string cardHolderName, string cardNumber, string expiryDate, string cvv, decimal initialBalance, Guid walletId)
    {
        CardHolderName = cardHolderName;
        CardNumber = cardNumber;
        ExpiryDate = expiryDate;
        Cvv = cvv;
        Balance = initialBalance;
        WalletId = walletId;
    }

    public void DeductBalance(decimal amount)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be greater than zero.");
        if (Balance < amount) throw new InvalidOperationException("Kartın limiti yetersiz.");
        Balance -= amount;
    }
}
