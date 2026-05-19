using MediatR;
using TRadeTurk.Application.Common.Interfaces;
using TRadeTurk.Application.DTOs;
using TRadeTurk.Domain.Entities;
using TRadeTurk.Domain.Enums;

namespace TRadeTurk.Application.Features.Assets.Commands;

public class BuyAssetCommandHandler : IRequestHandler<BuyAssetCommand, TransactionResultDto>
{
    private const decimal CommissionRate = 0.001m;

    private readonly IRepository<Wallet> _walletRepository;
    private readonly IRepository<Asset> _assetRepository;
    private readonly IRepository<Transaction> _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPriceProviderContext _priceProviderContext;
    private readonly ICurrentUserContext _currentUserContext;

    public BuyAssetCommandHandler(
        IRepository<Wallet> walletRepository,
        IRepository<Asset> assetRepository,
        IRepository<Transaction> transactionRepository,
        IUnitOfWork unitOfWork,
        IPriceProviderContext priceProviderContext,
        ICurrentUserContext currentUserContext)
    {
        _walletRepository = walletRepository;
        _assetRepository = assetRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
        _priceProviderContext = priceProviderContext;
        _currentUserContext = currentUserContext;
    }

    public async Task<TransactionResultDto> Handle(BuyAssetCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserContext.UserId ?? throw new InvalidOperationException("Oturum bulunamadi.");
        var symbol = request.Symbol.Trim().ToUpperInvariant();
        var wallet = (await _walletRepository.FindAsync(w => w.UserId == userId, cancellationToken)).FirstOrDefault();

        if (wallet == null)
        {
            return new TransactionResultDto { IsSuccess = false, Message = "Cuzdan bulunamadi." };
        }

        var actualPrice = await _priceProviderContext.GetCurrentPriceAsync(symbol, cancellationToken);
        if (actualPrice <= 0)
        {
            return new TransactionResultDto { IsSuccess = false, Message = "Guncel fiyat alinamadi." };
        }

        var slippagePercentage = (decimal)(Random.Shared.NextDouble() * (0.002 - 0.0005) + 0.0005);
        var executedPrice = actualPrice * (1 + slippagePercentage);
        var slippageDifference = executedPrice - request.RequestedPrice;

        var cost = request.Amount * executedPrice;
        var commission = cost * CommissionRate;
        var totalDeduct = cost + commission;

        try
        {
            wallet.DeductFiat(totalDeduct);
            _walletRepository.Update(wallet);

            var asset = (await _assetRepository.FindAsync(
                a => a.UserId == userId && a.WalletId == wallet.Id && a.Symbol == symbol,
                cancellationToken)).FirstOrDefault();

            if (asset == null)
            {
                asset = new Asset(userId, wallet.Id, symbol);
                asset.AddAmount(request.Amount, executedPrice);
                await _assetRepository.AddAsync(asset, cancellationToken);
            }
            else
            {
                asset.AddAmount(request.Amount, executedPrice);
                _assetRepository.Update(asset);
            }

            var transaction = new Transaction(userId, wallet.Id, TransactionType.Buy, symbol, request.Amount, executedPrice, commission, slippageDifference);
            transaction.MarkAsCompleted();
            await _transactionRepository.AddAsync(transaction, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new TransactionResultDto
            {
                IsSuccess = true,
                Message = "Alim basariyla gerceklesti.",
                TransactionId = transaction.Id,
                ExecutedPrice = executedPrice,
                CommissionUsed = commission,
                SlippageAmount = slippageDifference
            };
        }
        catch (InvalidOperationException ex)
        {
            return new TransactionResultDto { IsSuccess = false, Message = ex.Message };
        }
    }
}
