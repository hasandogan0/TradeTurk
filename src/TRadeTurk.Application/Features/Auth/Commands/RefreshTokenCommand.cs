using MediatR;
using TRadeTurk.Application.DTOs;

namespace TRadeTurk.Application.Features.Auth.Commands;

public class RefreshTokenCommand : IRequest<AuthResultDto>
{
    public string RefreshToken { get; set; } = string.Empty;
}
