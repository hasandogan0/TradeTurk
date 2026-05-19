using MediatR;
using TRadeTurk.Application.DTOs;

namespace TRadeTurk.Application.Features.Orders.Commands;

public class CancelOrderCommand : IRequest<OrderResultDto>
{
    public Guid Id { get; set; }
}
