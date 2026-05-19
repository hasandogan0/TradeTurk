using MediatR;
using TRadeTurk.Application.DTOs;

namespace TRadeTurk.Application.Features.Users.Commands;

public class UpdateCurrentUserCommand : IRequest<UserDto?>
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string PreferredCurrency { get; set; } = "USDT";
    public string ThemePreference { get; set; } = "dark";
}
