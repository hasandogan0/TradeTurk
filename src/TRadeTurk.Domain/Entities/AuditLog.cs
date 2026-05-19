using TRadeTurk.Domain.Common;

namespace TRadeTurk.Domain.Entities;

public class AuditLog : BaseEntity
{
    public Guid? UserId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string IpAddress { get; private set; } = string.Empty;
    public string UserAgent { get; private set; } = string.Empty;

    private AuditLog()
    {
    }

    public AuditLog(Guid? userId, string action, string ipAddress, string userAgent)
    {
        if (string.IsNullOrWhiteSpace(action)) throw new ArgumentException("Action cannot be empty.");

        UserId = userId;
        Action = action.Trim();
        IpAddress = ipAddress.Trim();
        UserAgent = userAgent.Trim();
    }
}
