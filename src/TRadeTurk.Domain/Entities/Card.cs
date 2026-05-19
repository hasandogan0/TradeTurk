using TRadeTurk.Domain.Common;

namespace TRadeTurk.Domain.Entities;

public class Card : BaseEntity
{
    public Guid UserId { get; private set; }
    public string CardHolderName { get; private set; }
    public string CardNumber { get; private set; }
    public int ExpiryMonth { get; private set; }
    public int ExpiryYear { get; private set; }
    public string CvvHash { get; private set; }
    public decimal Balance { get; private set; }

    public Guid WalletId { get; private set; }
    public Wallet Wallet { get; private set; } = null!;

    private Card()
    {
        CardHolderName = string.Empty;
        CardNumber = string.Empty;
        CvvHash = string.Empty;
    }

    public Card(Guid userId, string cardHolderName, string cardNumber, int expiryMonth, int expiryYear, string cvvHash, decimal initialBalance, Guid walletId)
    {
        if (userId == Guid.Empty) throw new ArgumentException("UserId must be valid.");
        if (walletId == Guid.Empty) throw new ArgumentException("WalletId must be valid.");
        if (string.IsNullOrWhiteSpace(cardHolderName)) throw new ArgumentException("Card holder name cannot be empty.");
        if (string.IsNullOrWhiteSpace(cardNumber) || cardNumber.Length != 16) throw new ArgumentException("Card number must have 16 digits.");
        if (expiryMonth is < 1 or > 12) throw new ArgumentException("Expiry month must be between 1 and 12.");
        if (expiryYear < DateTime.UtcNow.Year) throw new ArgumentException("Expiry year must be valid.");
        if (string.IsNullOrWhiteSpace(cvvHash)) throw new ArgumentException("CVV hash cannot be empty.");
        if (initialBalance < 0) throw new ArgumentException("Initial balance cannot be negative.");

        UserId = userId;
        CardHolderName = cardHolderName.Trim();
        CardNumber = cardNumber;
        ExpiryMonth = expiryMonth;
        ExpiryYear = expiryYear;
        CvvHash = cvvHash;
        Balance = initialBalance;
        WalletId = walletId;
    }

    public void DeductBalance(decimal amount)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be greater than zero.");
        if (Balance < amount) throw new InvalidOperationException("Kart limiti yetersiz.");
        Balance -= amount;
    }
}
