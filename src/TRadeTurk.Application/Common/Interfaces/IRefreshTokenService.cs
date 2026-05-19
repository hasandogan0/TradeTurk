namespace TRadeTurk.Application.Common.Interfaces;

public interface IRefreshTokenService
{
    string CreateRefreshToken();
    string Hash(string refreshToken);
    DateTime GetExpiry();
}
