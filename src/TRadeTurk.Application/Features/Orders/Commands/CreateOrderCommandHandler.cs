using MediatR;
using TRadeTurk.Application.Common;
using TRadeTurk.Application.Common.Interfaces;
using TRadeTurk.Application.DTOs;
using TRadeTurk.Domain.Entities;

namespace TRadeTurk.Application.Features.Orders.Commands;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderResultDto>
{
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<Wallet> _walletRepository;
    private readonly IRepository<Asset> _assetRepository;
    private readonly IRepository<Transaction> _transactionRepository;
    private readonly IPriceProviderContext _priceProviderContext;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public CreateOrderCommandHandler(
        IRepository<Order> orderRepository,
        IRepository<Wallet> walletRepository,
        IRepository<Asset> assetRepository,
        IRepository<Transaction> transactionRepository,
        IPriceProviderContext priceProviderContext,
        ICurrentUserContext currentUserContext,
        IUnitOfWork unitOfWork,
        IAuditService auditService)
    {
        _orderRepository = orderRepository;
        _walletRepository = walletRepository;
        _assetRepository = assetRepository;
        _transactionRepository = transactionRepository;
        _priceProviderContext = priceProviderContext;
        _currentUserContext = currentUserContext;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<OrderResultDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserContext.UserId ?? throw new InvalidOperationException("Oturum bulunamadi.");
        var symbol = request.Symbol.Trim().ToUpperInvariant();
        if (!MarketSymbols.IsSupported(symbol))
        {
            return new OrderResultDto { IsSuccess = false, Message = "Desteklenmeyen market sembolu." };
        }

        try
        {
            var side = OrderParsing.ParseSide(request.Side);
            var type = OrderParsing.ParseType(request.Type);
            var currentPrice = await _priceProviderContext.GetCurrentPriceAsync(symbol, cancellationToken);
            if (currentPrice <= 0) return new OrderResultDto { IsSuccess = false, Message = "Guncel fiyat alinamadi." };

            var orderPrice = type == Domain.Enums.OrderType.Market ? currentPrice : request.Price;
            var order = new Order(userId, symbol, side, type, request.Quantity, orderPrice, request.TriggerPrice);
            if (type != Domain.Enums.OrderType.Market)
            {
                order.MarkPending();
            }

            var executor = new OrderExecutionService(_walletRepository, _assetRepository, _transactionRepository);
            var validationError = await executor.ValidateAsync(order, currentPrice, cancellationToken);
            if (validationError != null)
            {
                return new OrderResultDto { IsSuccess = false, Message = validationError };
            }

            if (OrderExecutionService.ShouldExecute(order, currentPrice))
            {
                await executor.ExecuteAsync(order, currentPrice, cancellationToken);
            }

            await _orderRepository.AddAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _auditService.LogAsync(userId, $"OrderCreate {symbol} {request.Side} {request.Type}", cancellationToken);

            return new OrderResultDto
            {
                IsSuccess = true,
                Message = order.Status == Domain.Enums.OrderStatus.Filled ? "Emir basariyla gerceklesti." : "Emir defterine eklendi.",
                Order = OrderMapping.ToDto(order)
            };
        }
        catch (InvalidOperationException ex)
        {
            return new OrderResultDto { IsSuccess = false, Message = ex.Message };
        }
        catch (ArgumentException ex)
        {
            return new OrderResultDto { IsSuccess = false, Message = ex.Message };
        }
    }
}
