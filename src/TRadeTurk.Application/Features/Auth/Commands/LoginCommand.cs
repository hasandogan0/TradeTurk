using MediatR;
using TRadeTurk.Application.DTOs;

namespace TRadeTurk.Application.Features.Auth.Commands;

public class LoginCommand : IRequest<AuthResultDto>
{
    public string EmailOrUserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
