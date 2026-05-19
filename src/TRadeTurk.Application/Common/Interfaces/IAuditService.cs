namespace TRadeTurk.Application.Common.Interfaces;

public interface IAuditService
{
    Task LogAsync(Guid? userId, string action, CancellationToken cancellationToken = default);
}
