using MediatR;
using TRadeTurk.Application.Common.Interfaces;
using TRadeTurk.Domain.Entities;
using TRadeTurk.Domain.Enums;

namespace TRadeTurk.Application.Features.Orders.Commands;

public class ProcessPendingOrdersCommandHandler : IRequestHandler<ProcessPendingOrdersCommand, int>
{
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<Wallet> _walletRepository;
    private readonly IRepository<Asset> _assetRepository;
    private readonly IRepository<Transaction> _transactionRepository;
    private readonly IPriceProviderContext _priceProviderContext;
    private readonly IUnitOfWork _unitOfWork;

    public ProcessPendingOrdersCommandHandler(
        IRepository<Order> orderRepository,
        IRepository<Wallet> walletRepository,
        IRepository<Asset> assetRepository,
        IRepository<Transaction> transactionRepository,
        IPriceProviderContext priceProviderContext,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _walletRepository = walletRepository;
        _assetRepository = assetRepository;
        _transactionRepository = transactionRepository;
        _priceProviderContext = priceProviderContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(ProcessPendingOrdersCommand request, CancellationToken cancellationToken)
    {
        var pendingOrders = (await _orderRepository.FindAsync(o => o.Status == OrderStatus.Pending, cancellationToken))
            .OrderBy(o => o.CreatedAt)
            .ToArray();
        var executor = new OrderExecutionService(_walletRepository, _assetRepository, _transactionRepository);
        var executed = 0;

        foreach (var order in pendingOrders)
        {
            var currentPrice = await _priceProviderContext.GetCurrentPriceAsync(order.Symbol, cancellationToken);
            if (!OrderExecutionService.ShouldExecute(order, currentPrice))
            {
                continue;
            }

            var validationError = await executor.ValidateAsync(order, currentPrice, cancellationToken);
            if (validationError != null)
            {
                order.Fail();
                _orderRepository.Update(order);
                continue;
            }

            await executor.ExecuteAsync(order, currentPrice, cancellationToken);
            _orderRepository.Update(order);
            executed++;
        }

        if (executed > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return executed;
    }
}
