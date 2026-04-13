using MediatR;
using TRadeTurk.Application.DTOs;
using TRadeTurk.Domain.Entities;
using TRadeTurk.Domain.Enums;
using TRadeTurk.Domain.Interfaces;

namespace TRadeTurk.Application.Features.Assets.Commands;

public class BuyAssetCommandHandler : IRequestHandler<BuyAssetCommand, TransactionResultDto>
{
    private readonly IRepository<Wallet> _walletRepository;
    private readonly IRepository<Asset> _assetRepository;
    private readonly IRepository<Transaction> _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBinanceService _binanceService;

    private const decimal CommissionRate = 0.001m; // %0.1 Komisyon

    public BuyAssetCommandHandler(
        IRepository<Wallet> walletRepository,
        IRepository<Asset> assetRepository,
        IRepository<Transaction> transactionRepository,
        IUnitOfWork unitOfWork,
        IBinanceService binanceService)
    {
        _walletRepository = walletRepository;
        _assetRepository = assetRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
        _binanceService = binanceService;
    }

    public async Task<TransactionResultDto> Handle(BuyAssetCommand request, CancellationToken cancellationToken)
    {
        var wallet = await _walletRepository.GetByIdAsync(request.WalletId, cancellationToken);
        if (wallet == null) return new TransactionResultDto { IsSuccess = false, Message = "Cüzdan bulunamadı." };

        // 1. İşlem Ağ Onay Gecikmesi (Slippage ortamı oluşturmak için)
        await Task.Delay(1000, cancellationToken);

        // 2. Gerçek Fiyatı Al ve Slippage Simülasyonu (%0.05 ile %0.2 arası fiyat kayması)
        decimal actualPrice = await _binanceService.GetCurrentPriceAsync(request.Symbol, cancellationToken);
        decimal slippagePercentage = (decimal)(new Random().NextDouble() * (0.002 - 0.0005) + 0.0005);
        
        decimal executedPrice = actualPrice * (1 + slippagePercentage); // Alımda fiyat kullanıcı aleyhine artar
        decimal slippageDifference = executedPrice - request.RequestedPrice;

        // 3. Maliyet Hesaplamaları
        decimal cost = request.Amount * executedPrice;
        decimal commission = cost * CommissionRate;
        decimal totalDeduct = cost + commission;

        try
        {
            // Bakiye düş (Yetersizse Exception fırlatır)
            wallet.DeductFiat(totalDeduct);
            _walletRepository.Update(wallet);

            // Asset yönetimi
            var existingAssets = await _assetRepository.FindAsync(a => a.WalletId == request.WalletId && a.Symbol == request.Symbol, cancellationToken);
            var asset = existingAssets.FirstOrDefault();
            
            if (asset == null)
            {
                asset = new Asset(wallet.Id, request.Symbol);
                asset.AddAmount(request.Amount, executedPrice);
                await _assetRepository.AddAsync(asset, cancellationToken);
            }
            else
            {
                asset.AddAmount(request.Amount, executedPrice);
                _assetRepository.Update(asset);
            }

            // Transaction History kaydı
            var transaction = new Transaction(wallet.Id, TransactionType.Buy, request.Symbol, request.Amount, executedPrice, commission, slippageDifference);
            transaction.MarkAsCompleted();
            await _transactionRepository.AddAsync(transaction, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new TransactionResultDto
            {
                IsSuccess = true,
                Message = "Alım başarıyla gerçekleşti.",
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
