using TRadeTurk.Domain.Common;

namespace TRadeTurk.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    private RefreshToken()
    {
    }

    public RefreshToken(Guid userId, string tokenHash, DateTime expiresAt)
    {
        if (userId == Guid.Empty) throw new ArgumentException("UserId must be valid.");
        if (string.IsNullOrWhiteSpace(tokenHash)) throw new ArgumentException("Token hash cannot be empty.");

        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
    }

    public bool IsActive => RevokedAt == null && ExpiresAt > DateTime.UtcNow;

    public void Revoke()
    {
        RevokedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
