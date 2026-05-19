using MediatR;
using TRadeTurk.Application.Common.Interfaces;
using TRadeTurk.Application.DTOs;
using TRadeTurk.Application.Features.Orders;
using TRadeTurk.Domain.Entities;
using TRadeTurk.Domain.Enums;

namespace TRadeTurk.Application.Features.Orders.Queries;

public class GetOrderHistoryQueryHandler : IRequestHandler<GetOrderHistoryQuery, IReadOnlyCollection<OrderDto>>
{
    private readonly IRepository<Order> _orderRepository;
    private readonly ICurrentUserContext _currentUserContext;

    public GetOrderHistoryQueryHandler(IRepository<Order> orderRepository, ICurrentUserContext currentUserContext)
    {
        _orderRepository = orderRepository;
        _currentUserContext = currentUserContext;
    }

    public async Task<IReadOnlyCollection<OrderDto>> Handle(GetOrderHistoryQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserContext.UserId ?? throw new InvalidOperationException("Oturum bulunamadi.");
        return (await _orderRepository.FindAsync(o => o.UserId == userId && o.Status != OrderStatus.Pending, cancellationToken))
            .OrderByDescending(o => o.CreatedAt)
            .Select(OrderMapping.ToDto)
            .ToArray();
    }
}
