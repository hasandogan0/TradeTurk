using MediatR;
using TRadeTurk.Application.Common.Interfaces;
using TRadeTurk.Application.DTOs;
using TRadeTurk.Domain.Entities;

namespace TRadeTurk.Application.Features.Portfolio.Queries;

public class GetPortfolioSummaryQueryHandler : IRequestHandler<GetPortfolioSummaryQuery, PortfolioSummaryDto?>
{
    private readonly IRepository<Wallet> _walletRepository;
    private readonly IRepository<Asset> _assetRepository;
    private readonly IPriceProviderContext _priceProviderContext;

    public GetPortfolioSummaryQueryHandler(IRepository<Wallet> walletRepository, IRepository<Asset> assetRepository, IPriceProviderContext priceProviderContext)
    {
        _walletRepository = walletRepository;
        _assetRepository = assetRepository;
        _priceProviderContext = priceProviderContext;
    }

    public async Task<PortfolioSummaryDto?> Handle(GetPortfolioSummaryQuery request, CancellationToken cancellationToken)
    {
        var wallet = (await _walletRepository.FindAsync(w => w.UserId == request.UserId, cancellationToken)).FirstOrDefault();
        if (wallet == null) return null;

        var assets = await _assetRepository.FindAsync(a => a.UserId == request.UserId && a.Amount > 0, cancellationToken);
        var allocations = new List<AssetAllocationDto>();
        decimal assetValue = 0;
        decimal totalCost = 0;

        foreach (var asset in assets)
        {
            var price = await _priceProviderContext.GetCurrentPriceAsync(asset.Symbol, cancellationToken);
            if (price <= 0) price = asset.AverageCost;
            var value = asset.Amount * price;
            var cost = asset.Amount * asset.AverageCost;
            assetValue += value;
            totalCost += cost;
            allocations.Add(new AssetAllocationDto
            {
                Symbol = asset.Symbol,
                Amount = asset.Amount,
                AverageCost = asset.AverageCost,
                CurrentPrice = price,
                Value = value,
                UnrealizedPnl = value - cost
            });
        }

        var totalPortfolio = wallet.FiatBalance + assetValue;
        foreach (var allocation in allocations)
        {
            allocation.AllocationPercent = totalPortfolio == 0 ? 0 : allocation.Value / totalPortfolio * 100;
        }

        return new PortfolioSummaryDto
        {
            AvailableUsdt = wallet.FiatBalance,
            TotalAssetValue = assetValue,
            TotalPortfolioValue = totalPortfolio,
            UnrealizedPnl = assetValue - totalCost,
            AssetAllocation = allocations
        };
    }
}
