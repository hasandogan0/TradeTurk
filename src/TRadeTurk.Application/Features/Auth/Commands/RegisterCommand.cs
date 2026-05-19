using MediatR;
using TRadeTurk.Application.DTOs;

namespace TRadeTurk.Application.Features.Auth.Commands;

public class RegisterCommand : IRequest<AuthResultDto>
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
