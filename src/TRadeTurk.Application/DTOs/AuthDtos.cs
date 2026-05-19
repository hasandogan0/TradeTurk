namespace TRadeTurk.Application.DTOs;

public class AuthResultDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public UserDto User { get; set; } = new();
}

public class UserDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string PreferredCurrency { get; set; } = "USDT";
    public string ThemePreference { get; set; } = "dark";
}
