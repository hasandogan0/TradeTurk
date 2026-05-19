using TRadeTurk.Application.Common.Interfaces;
using TRadeTurk.Domain.Entities;
using TRadeTurk.Domain.Enums;

namespace TRadeTurk.Application.Features.Orders;

internal sealed class OrderExecutionService
{
    private const decimal CommissionRate = 0.001m;

    private readonly IRepository<Wallet> _walletRepository;
    private readonly IRepository<Asset> _assetRepository;
    private readonly IRepository<Transaction> _transactionRepository;

    public OrderExecutionService(
        IRepository<Wallet> walletRepository,
        IRepository<Asset> assetRepository,
        IRepository<Transaction> transactionRepository)
    {
        _walletRepository = walletRepository;
        _assetRepository = assetRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<string?> ValidateAsync(Order order, decimal referencePrice, CancellationToken cancellationToken)
    {
        var wallet = (await _walletRepository.FindAsync(w => w.UserId == order.UserId, cancellationToken)).FirstOrDefault();
        if (wallet == null) return "Cuzdan bulunamadi.";

        if (order.Side == OrderSide.Buy)
        {
            var estimatedPrice = order.Price ?? order.TriggerPrice ?? referencePrice;
            var estimatedTotal = order.Quantity * estimatedPrice * (1 + CommissionRate);
            return wallet.FiatBalance >= estimatedTotal ? null : "Yetersiz USDT bakiyesi.";
        }

        var asset = (await _assetRepository.FindAsync(a => a.UserId == order.UserId && a.WalletId == wallet.Id && a.Symbol == order.Symbol, cancellationToken)).FirstOrDefault();
        return asset != null && asset.Amount >= order.Quantity ? null : "Yetersiz kripto varlik.";
    }

    public async Task ExecuteAsync(Order order, decimal executedPrice, CancellationToken cancellationToken)
    {
        var wallet = (await _walletRepository.FindAsync(w => w.UserId == order.UserId, cancellationToken)).FirstOrDefault()
            ?? throw new InvalidOperationException("Cuzdan bulunamadi.");

        var commissionBase = order.Quantity * executedPrice;
        var commission = commissionBase * CommissionRate;

        if (order.Side == OrderSide.Buy)
        {
            wallet.DeductFiat(commissionBase + commission);
            _walletRepository.Update(wallet);

            var asset = (await _assetRepository.FindAsync(a => a.UserId == order.UserId && a.WalletId == wallet.Id && a.Symbol == order.Symbol, cancellationToken)).FirstOrDefault();
            if (asset == null)
            {
                asset = new Asset(order.UserId, wallet.Id, order.Symbol);
                asset.AddAmount(order.Quantity, executedPrice);
                await _assetRepository.AddAsync(asset, cancellationToken);
            }
            else
            {
                asset.AddAmount(order.Quantity, executedPrice);
                _assetRepository.Update(asset);
            }
        }
        else
        {
            var asset = (await _assetRepository.FindAsync(a => a.UserId == order.UserId && a.WalletId == wallet.Id && a.Symbol == order.Symbol, cancellationToken)).FirstOrDefault()
                ?? throw new InvalidOperationException("Yetersiz kripto varlik.");

            asset.DeductAmount(order.Quantity);
            _assetRepository.Update(asset);
            wallet.AddFiat(commissionBase - commission);
            _walletRepository.Update(wallet);
        }

        var transactionType = order.Side == OrderSide.Buy ? TransactionType.Buy : TransactionType.Sell;
        var transaction = new Transaction(order.UserId, wallet.Id, transactionType, order.Symbol, order.Quantity, executedPrice, commission, 0m);
        transaction.MarkAsCompleted();
        await _transactionRepository.AddAsync(transaction, cancellationToken);

        order.Fill(executedPrice);
    }

    public static bool ShouldExecute(Order order, decimal currentPrice)
    {
        return order.Type switch
        {
            OrderType.Market => true,
            OrderType.Limit when order.Side == OrderSide.Buy => currentPrice <= order.Price,
            OrderType.Limit when order.Side == OrderSide.Sell => currentPrice >= order.Price,
            OrderType.StopLoss when order.Side == OrderSide.Sell => currentPrice <= order.TriggerPrice,
            OrderType.StopLoss when order.Side == OrderSide.Buy => currentPrice >= order.TriggerPrice,
            OrderType.TakeProfit when order.Side == OrderSide.Sell => currentPrice >= order.TriggerPrice,
            OrderType.TakeProfit when order.Side == OrderSide.Buy => currentPrice <= order.TriggerPrice,
            _ => false
        };
    }
}
