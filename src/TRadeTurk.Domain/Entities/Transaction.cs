using TRadeTurk.Domain.Common;
using TRadeTurk.Domain.Enums;

namespace TRadeTurk.Domain.Entities;

public class Transaction : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid WalletId { get; private set; }
    public Wallet Wallet { get; private set; } = null!;

    public TransactionType Type { get; private set; }
    public TransactionStatus Status { get; private set; }
    
    public string? Symbol { get; private set; } // e.g., "BTCUSDT"
    public decimal Amount { get; private set; } 
    public decimal Price { get; private set; }  
    public decimal Commission { get; private set; } 
    public decimal Slippage { get; private set; }   

    private Transaction() { } // EF Core

    public Transaction(Guid userId, Guid walletId, TransactionType type, string? symbol, decimal amount, decimal price, decimal commission, decimal slippage)
    {
        if (userId == Guid.Empty) throw new ArgumentException("UserId must be valid.");
        if (walletId == Guid.Empty) throw new ArgumentException("WalletId must be valid.");
        if (amount <= 0) throw new ArgumentException("Amount must be greater than zero.");
        if (price <= 0) throw new ArgumentException("Price must be greater than zero.");
        if (commission < 0) throw new ArgumentException("Commission cannot be negative.");

        UserId = userId;
        WalletId = walletId;
        Type = type;
        Symbol = symbol?.Trim().ToUpperInvariant();
        Amount = amount;
        Price = price;
        Commission = commission;
        Slippage = slippage;
        Status = TransactionStatus.Pending;
    }

    public void MarkAsCompleted()
    {
        Status = TransactionStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void MarkAsFailed()
    {
        Status = TransactionStatus.Failed;
        UpdatedAt = DateTime.UtcNow;
    }
}
