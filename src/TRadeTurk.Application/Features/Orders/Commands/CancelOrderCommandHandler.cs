using MediatR;
using TRadeTurk.Application.Common.Interfaces;
using TRadeTurk.Application.DTOs;
using TRadeTurk.Domain.Entities;

namespace TRadeTurk.Application.Features.Orders.Commands;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, OrderResultDto>
{
    private readonly IRepository<Order> _orderRepository;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public CancelOrderCommandHandler(IRepository<Order> orderRepository, ICurrentUserContext currentUserContext, IUnitOfWork unitOfWork, IAuditService auditService)
    {
        _orderRepository = orderRepository;
        _currentUserContext = currentUserContext;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<OrderResultDto> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserContext.UserId ?? throw new InvalidOperationException("Oturum bulunamadi.");
        var order = (await _orderRepository.FindAsync(o => o.Id == request.Id && o.UserId == userId, cancellationToken)).FirstOrDefault();
        if (order == null)
        {
            return new OrderResultDto { IsSuccess = false, Message = "Emir bulunamadi." };
        }

        try
        {
            order.Cancel();
            _orderRepository.Update(order);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _auditService.LogAsync(userId, $"OrderCancel {order.Symbol}", cancellationToken);
            return new OrderResultDto { IsSuccess = true, Message = "Emir iptal edildi.", Order = OrderMapping.ToDto(order) };
        }
        catch (InvalidOperationException ex)
        {
            return new OrderResultDto { IsSuccess = false, Message = ex.Message, Order = OrderMapping.ToDto(order) };
        }
    }
}
