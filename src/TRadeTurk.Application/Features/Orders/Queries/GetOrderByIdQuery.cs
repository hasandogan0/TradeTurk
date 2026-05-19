using MediatR;
using TRadeTurk.Application.DTOs;

namespace TRadeTurk.Application.Features.Orders.Queries;

public class GetOrderByIdQuery : IRequest<OrderDto?>
{
    public Guid Id { get; set; }
}
