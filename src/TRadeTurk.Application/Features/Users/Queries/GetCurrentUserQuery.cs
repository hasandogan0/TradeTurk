using MediatR;
using TRadeTurk.Application.DTOs;

namespace TRadeTurk.Application.Features.Users.Queries;

public class GetCurrentUserQuery : IRequest<UserDto?>
{
    public Guid UserId { get; set; }
}
