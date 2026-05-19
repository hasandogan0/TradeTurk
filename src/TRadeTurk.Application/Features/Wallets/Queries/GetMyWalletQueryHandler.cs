using MediatR;
using TRadeTurk.Application.Common.Interfaces;
using TRadeTurk.Application.DTOs;
using TRadeTurk.Domain.Entities;

namespace TRadeTurk.Application.Features.Wallets.Queries;

public class GetMyWalletQueryHandler : IRequestHandler<GetMyWalletQuery, WalletDetailsDto?>
{
    private readonly IRepository<Wallet> _walletRepository;
    private readonly IRepository<Asset> _assetRepository;
    private readonly IRepository<Card> _cardRepository;
    private readonly IPriceProviderContext _priceProviderContext;

    public GetMyWalletQueryHandler(
        IRepository<Wallet> walletRepository,
        IRepository<Asset> assetRepository,
        IRepository<Card> cardRepository,
        IPriceProviderContext priceProviderContext)
    {
        _walletRepository = walletRepository;
        _assetRepository = assetRepository;
        _cardRepository = cardRepository;
        _priceProviderContext = priceProviderContext;
    }

    public async Task<WalletDetailsDto?> Handle(GetMyWalletQuery request, CancellationToken cancellationToken)
    {
        var wallet = (await _walletRepository.FindAsync(w => w.UserId == request.UserId, cancellationToken)).FirstOrDefault();
        if (wallet == null) return null;

        var assets = (await _assetRepository.FindAsync(a => a.UserId == request.UserId && a.WalletId == wallet.Id && a.Amount > 0, cancellationToken)).ToArray();
        var card = (await _cardRepository.FindAsync(c => c.UserId == request.UserId && c.WalletId == wallet.Id, cancellationToken)).FirstOrDefault();

        var assetDtos = new List<AssetDto>();
        decimal assetValue = 0;
        foreach (var asset in assets)
        {
            var price = await _priceProviderContext.GetCurrentPriceAsync(asset.Symbol, cancellationToken);
            if (price <= 0) price = asset.AverageCost;
            assetValue += asset.Amount * price;
            assetDtos.Add(new AssetDto
            {
                Id = asset.Id,
                UserId = asset.UserId,
                WalletId = asset.WalletId,
                Symbol = asset.Symbol,
                Amount = asset.Amount,
                AverageCost = asset.AverageCost
            });
        }

        return new WalletDetailsDto
        {
            Id = wallet.Id,
            AvailableBalance = wallet.FiatBalance,
            TotalBalance = wallet.FiatBalance,
            Assets = assetDtos,
            PortfolioTotalValue = wallet.FiatBalance + assetValue,
            VirtualCard = card == null ? null : new CardDto
            {
                CardHolderName = card.CardHolderName,
                MaskedCardNumber = $"**** **** **** {card.CardNumber[^4..]}",
                ExpiryMonth = card.ExpiryMonth,
                ExpiryYear = card.ExpiryYear
            }
        };
    }
}
