using TRadeTurk.Application.Common.Interfaces;

namespace TRadeTurk.Infrastructure.Services;

public class CurrentUserContext : ICurrentUserContext
{
    public Guid? UserId { get; private set; }

    public void SetUserId(Guid userId)
    {
        if (userId == Guid.Empty) throw new ArgumentException("UserId must be valid.");

        UserId = userId;
    }
}
