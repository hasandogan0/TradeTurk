using MediatR;
using TRadeTurk.Application.Common.Interfaces;
using TRadeTurk.Application.DTOs;
using TRadeTurk.Application.Features.Orders;
using TRadeTurk.Domain.Entities;

namespace TRadeTurk.Application.Features.Orders.Queries;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto?>
{
    private readonly IRepository<Order> _orderRepository;
    private readonly ICurrentUserContext _currentUserContext;

    public GetOrderByIdQueryHandler(IRepository<Order> orderRepository, ICurrentUserContext currentUserContext)
    {
        _orderRepository = orderRepository;
        _currentUserContext = currentUserContext;
    }

    public async Task<OrderDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserContext.UserId ?? throw new InvalidOperationException("Oturum bulunamadi.");
        var order = (await _orderRepository.FindAsync(o => o.Id == request.Id && o.UserId == userId, cancellationToken)).FirstOrDefault();
        return order == null ? null : OrderMapping.ToDto(order);
    }
}
