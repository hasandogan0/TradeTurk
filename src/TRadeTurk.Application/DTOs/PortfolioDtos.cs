namespace TRadeTurk.Application.DTOs;

public class CardDto
{
    public string CardHolderName { get; set; } = string.Empty;
    public string MaskedCardNumber { get; set; } = string.Empty;
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
}

public class WalletDetailsDto
{
    public Guid Id { get; set; }
    public decimal TotalBalance { get; set; }
    public decimal AvailableBalance { get; set; }
    public CardDto? VirtualCard { get; set; }
    public IReadOnlyCollection<AssetDto> Assets { get; set; } = Array.Empty<AssetDto>();
    public decimal PortfolioTotalValue { get; set; }
}

public class TransactionDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Price { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class PortfolioSummaryDto
{
    public decimal TotalPortfolioValue { get; set; }
    public decimal AvailableUsdt { get; set; }
    public decimal TotalAssetValue { get; set; }
    public decimal UnrealizedPnl { get; set; }
    public IReadOnlyCollection<AssetAllocationDto> AssetAllocation { get; set; } = Array.Empty<AssetAllocationDto>();
}

public class AssetAllocationDto
{
    public string Symbol { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal AverageCost { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal Value { get; set; }
    public decimal AllocationPercent { get; set; }
    public decimal UnrealizedPnl { get; set; }
}
