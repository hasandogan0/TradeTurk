namespace TRadeTurk.Application.Common.Interfaces;

public interface ICurrentUserContext
{
    Guid? UserId { get; }
    void SetUserId(Guid userId);
}
