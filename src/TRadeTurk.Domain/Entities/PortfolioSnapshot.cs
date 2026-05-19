using TRadeTurk.Domain.Common;

namespace TRadeTurk.Domain.Entities;

public class PortfolioSnapshot : BaseEntity
{
    public Guid UserId { get; private set; }
    public decimal TotalValue { get; private set; }
    public decimal AvailableUSDT { get; private set; }
    public decimal AssetValue { get; private set; }
    public decimal TotalPnL { get; private set; }

    private PortfolioSnapshot()
    {
    }

    public PortfolioSnapshot(Guid userId, decimal totalValue, decimal availableUsdt, decimal assetValue, decimal totalPnl)
    {
        if (userId == Guid.Empty) throw new ArgumentException("UserId must be valid.");

        UserId = userId;
        TotalValue = totalValue;
        AvailableUSDT = availableUsdt;
        AssetValue = assetValue;
        TotalPnL = totalPnl;
    }
}
