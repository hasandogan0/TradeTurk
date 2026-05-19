using TRadeTurk.Domain.Common;

namespace TRadeTurk.Domain.Entities;

public class User : BaseEntity
{
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string UserName { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string PreferredCurrency { get; private set; } = "USDT";
    public string ThemePreference { get; private set; } = "dark";

    private User()
    {
    }

    public User(string fullName, string email, string userName, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("Full name cannot be empty.");
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email cannot be empty.");
        if (string.IsNullOrWhiteSpace(userName)) throw new ArgumentException("User name cannot be empty.");
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Password hash cannot be empty.");

        FullName = fullName.Trim();
        Email = email.Trim().ToLowerInvariant();
        UserName = userName.Trim();
        PasswordHash = passwordHash;
    }

    public void UpdateProfile(string fullName, string email, string userName, string preferredCurrency, string themePreference)
    {
        if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("Full name cannot be empty.");
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email cannot be empty.");
        if (string.IsNullOrWhiteSpace(userName)) throw new ArgumentException("User name cannot be empty.");
        if (string.IsNullOrWhiteSpace(preferredCurrency)) throw new ArgumentException("Preferred currency cannot be empty.");
        if (string.IsNullOrWhiteSpace(themePreference)) throw new ArgumentException("Theme preference cannot be empty.");

        FullName = fullName.Trim();
        Email = email.Trim().ToLowerInvariant();
        UserName = userName.Trim();
        PreferredCurrency = preferredCurrency.Trim().ToUpperInvariant();
        ThemePreference = themePreference.Trim().ToLowerInvariant();
    }

    public void ChangePasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Password hash cannot be empty.");

        PasswordHash = passwordHash;
    }
}
