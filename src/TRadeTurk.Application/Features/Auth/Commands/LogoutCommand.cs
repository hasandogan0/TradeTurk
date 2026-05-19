using MediatR;

namespace TRadeTurk.Application.Features.Auth.Commands;

public class LogoutCommand : IRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
