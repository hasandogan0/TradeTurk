using MediatR;
using TRadeTurk.Application.DTOs;

namespace TRadeTurk.Application.Features.Orders.Queries;

public class GetOrderHistoryQuery : IRequest<IReadOnlyCollection<OrderDto>>
{
}
