using MediatR;
using TRadeTurk.Application.Common.Interfaces;
using TRadeTurk.Application.DTOs;
using TRadeTurk.Domain.Entities;

namespace TRadeTurk.Application.Features.Auth.Commands;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResultDto>
{
    private readonly IRepository<RefreshToken> _refreshTokenRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokenCommandHandler(
        IRepository<RefreshToken> refreshTokenRepository,
        IRepository<User> userRepository,
        IRefreshTokenService refreshTokenService,
        ITokenService tokenService,
        IUnitOfWork unitOfWork)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _refreshTokenService = refreshTokenService;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResultDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var hash = _refreshTokenService.Hash(request.RefreshToken);
        var token = (await _refreshTokenRepository.FindAsync(t => t.TokenHash == hash, cancellationToken)).FirstOrDefault();
        if (token == null || !token.IsActive)
        {
            throw new InvalidOperationException("Refresh token gecersiz veya suresi dolmus.");
        }

        var user = await _userRepository.GetByIdAsync(token.UserId, cancellationToken)
            ?? throw new InvalidOperationException("Kullanici bulunamadi.");

        token.Revoke();
        _refreshTokenRepository.Update(token);

        var nextRefreshToken = _refreshTokenService.CreateRefreshToken();
        await _refreshTokenRepository.AddAsync(new RefreshToken(user.Id, _refreshTokenService.Hash(nextRefreshToken), _refreshTokenService.GetExpiry()), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResultDto
        {
            Token = _tokenService.CreateToken(user),
            RefreshToken = nextRefreshToken,
            User = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                UserName = user.UserName,
                PreferredCurrency = user.PreferredCurrency,
                ThemePreference = user.ThemePreference
            }
        };
    }
}
