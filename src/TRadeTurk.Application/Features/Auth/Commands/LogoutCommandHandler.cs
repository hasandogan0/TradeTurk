using MediatR;
using TRadeTurk.Application.Common.Interfaces;
using TRadeTurk.Domain.Entities;

namespace TRadeTurk.Application.Features.Auth.Commands;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IRepository<RefreshToken> _refreshTokenRepository;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public LogoutCommandHandler(IRepository<RefreshToken> refreshTokenRepository, IRefreshTokenService refreshTokenService, IUnitOfWork unitOfWork, IAuditService auditService)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _refreshTokenService = refreshTokenService;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken)) return;

        var hash = _refreshTokenService.Hash(request.RefreshToken);
        var token = (await _refreshTokenRepository.FindAsync(t => t.TokenHash == hash, cancellationToken)).FirstOrDefault();
        if (token == null || !token.IsActive) return;

        token.Revoke();
        _refreshTokenRepository.Update(token);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync(token.UserId, "Logout", cancellationToken);
    }
}
