using System.Security.Cryptography;
using System.Text;
using TRadeTurk.Application.Common.Interfaces;

namespace TRadeTurk.Infrastructure.Services;

public class RefreshTokenService : IRefreshTokenService
{
    public string CreateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    public string Hash(string refreshToken)
    {
        using var sha = SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(refreshToken)));
    }

    public DateTime GetExpiry()
    {
        return DateTime.UtcNow.AddDays(14);
    }
}
