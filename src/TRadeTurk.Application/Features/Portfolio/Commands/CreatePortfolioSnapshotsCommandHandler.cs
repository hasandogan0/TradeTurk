using MediatR;
using TRadeTurk.Application.Common.Interfaces;
using TRadeTurk.Domain.Entities;

namespace TRadeTurk.Application.Features.Portfolio.Commands;

public class CreatePortfolioSnapshotsCommandHandler : IRequestHandler<CreatePortfolioSnapshotsCommand, int>
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Wallet> _walletRepository;
    private readonly IRepository<Asset> _assetRepository;
    private readonly IRepository<PortfolioSnapshot> _snapshotRepository;
    private readonly IPriceProviderContext _priceProviderContext;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePortfolioSnapshotsCommandHandler(
        IRepository<User> userRepository,
        IRepository<Wallet> walletRepository,
        IRepository<Asset> assetRepository,
        IRepository<PortfolioSnapshot> snapshotRepository,
        IPriceProviderContext priceProviderContext,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _walletRepository = walletRepository;
        _assetRepository = assetRepository;
        _snapshotRepository = snapshotRepository;
        _priceProviderContext = priceProviderContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(CreatePortfolioSnapshotsCommand request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        var created = 0;

        foreach (var user in users)
        {
            var wallet = (await _walletRepository.FindAsync(w => w.UserId == user.Id, cancellationToken)).FirstOrDefault();
            if (wallet == null) continue;

            var assets = await _assetRepository.FindAsync(a => a.UserId == user.Id && a.WalletId == wallet.Id, cancellationToken);
            var assetValue = 0m;
            var costBasis = 0m;

            foreach (var asset in assets)
            {
                var price = await _priceProviderContext.GetCurrentPriceAsync(asset.Symbol, cancellationToken);
                assetValue += asset.Amount * price;
                costBasis += asset.Amount * asset.AverageCost;
            }

            var totalValue = wallet.FiatBalance + assetValue;
            var totalPnl = assetValue - costBasis;
            await _snapshotRepository.AddAsync(new PortfolioSnapshot(user.Id, totalValue, wallet.FiatBalance, assetValue, totalPnl), cancellationToken);
            created++;
        }

        if (created > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return created;
    }
}
