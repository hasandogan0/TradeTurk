using Microsoft.AspNetCore.Http;
using TRadeTurk.Application.Common.Interfaces;
using TRadeTurk.Domain.Entities;

namespace TRadeTurk.Infrastructure.Services;

public class AuditLoggingService : IAuditService
{
    private readonly IRepository<AuditLog> _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditLoggingService(IRepository<AuditLog> auditLogRepository, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
    {
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(Guid? userId, string action, CancellationToken cancellationToken = default)
    {
        var context = _httpContextAccessor.HttpContext;
        var ip = context?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = context?.Request.Headers.UserAgent.ToString() ?? "unknown";

        var log = new AuditLog(userId, action, ip, userAgent);
        await _auditLogRepository.AddAsync(log, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
