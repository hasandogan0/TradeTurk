using MediatR;
using TRadeTurk.Application.Common.Interfaces;
using TRadeTurk.Application.DTOs;
using TRadeTurk.Domain.Entities;
using TRadeTurk.Domain.Enums;

namespace TRadeTurk.Application.Features.Assets.Commands;

public class SellAssetCommandHandler : IRequestHandler<SellAssetCommand, TransactionResultDto>
{
    private readonly IRepository<Wallet> _walletRepository;
    private readonly IRepository<Asset> _assetRepository;
    private readonly IRepository<Transaction> _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPriceProviderContext _priceProviderContext;

    private const decimal CommissionRate = 0.001m;

    public SellAssetCommandHandler(
        IRepository<Wallet> walletRepository,
        IRepository<Asset> assetRepository,
        IRepository<Transaction> transactionRepository,
        IUnitOfWork unitOfWork,
        IPriceProviderContext priceProviderContext)
    {
        _walletRepository = walletRepository;
        _assetRepository = assetRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
        _priceProviderContext = priceProviderContext;
    }

    public async Task<TransactionResultDto> Handle(SellAssetCommand request, CancellationToken cancellationToken)
    {
        var symbol = request.Symbol.Trim().ToUpperInvariant();
        var wallet = (await _walletRepository.FindAsync(w => w.UserId == request.UserId, cancellationToken)).FirstOrDefault();

        if (wallet == null)
        {
            return new TransactionResultDto { IsSuccess = false, Message = "Cuzdan bulunamadi." };
        }

        var asset = (await _assetRepository.FindAsync(
            a => a.UserId == request.UserId && a.WalletId == wallet.Id && a.Symbol == symbol,
            cancellationToken)).FirstOrDefault();

        if (asset == null || asset.Amount < request.Amount)
        {
            return new TransactionResultDto { IsSuccess = false, Message = "Yetersiz kripto varlik." };
        }

        var actualPrice = await _priceProviderContext.GetCurrentPriceAsync(symbol, cancellationToken);
        if (actualPrice <= 0)
        {
            return new TransactionResultDto { IsSuccess = false, Message = "Guncel fiyat alinamadi." };
        }

        var slippagePercentage = (decimal)(Random.Shared.NextDouble() * (0.002 - 0.0005) + 0.0005);
        var executedPrice = actualPrice * (1 - slippagePercentage);
        var slippageDifference = request.RequestedPrice - executedPrice;

        var revenue = request.Amount * executedPrice;
        var commission = revenue * CommissionRate;
        var netFiatAddition = revenue - commission;

        try
        {
            asset.DeductAmount(request.Amount);
            _assetRepository.Update(asset);

            wallet.AddFiat(netFiatAddition);
            _walletRepository.Update(wallet);

            var transaction = new Transaction(request.UserId, wallet.Id, TransactionType.Sell, symbol, request.Amount, executedPrice, commission, slippageDifference);
            transaction.MarkAsCompleted();
            await _transactionRepository.AddAsync(transaction, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new TransactionResultDto
            {
                IsSuccess = true,
                Message = "Satis basariyla gerceklesti.",
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
