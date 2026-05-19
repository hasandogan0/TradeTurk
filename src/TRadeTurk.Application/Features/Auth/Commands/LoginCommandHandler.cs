using MediatR;
using TRadeTurk.Application.Common.Interfaces;
using TRadeTurk.Application.DTOs;
using TRadeTurk.Domain.Entities;

namespace TRadeTurk.Application.Features.Auth.Commands;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResultDto>
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<RefreshToken> _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public LoginCommandHandler(
        IRepository<User> userRepository,
        IRepository<RefreshToken> refreshTokenRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IRefreshTokenService refreshTokenService,
        IUnitOfWork unitOfWork,
        IAuditService auditService)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _refreshTokenService = refreshTokenService;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<AuthResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var key = request.EmailOrUserName.Trim().ToLowerInvariant();
        var user = (await _userRepository.FindAsync(u => u.Email == key || u.UserName.ToLower() == key, cancellationToken)).FirstOrDefault();

        if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new InvalidOperationException("Email/kullanici adi veya sifre hatali.");
        }

        var refreshToken = _refreshTokenService.CreateRefreshToken();
        await _refreshTokenRepository.AddAsync(new RefreshToken(user.Id, _refreshTokenService.Hash(refreshToken), _refreshTokenService.GetExpiry()), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync(user.Id, "Login", cancellationToken);

        return new AuthResultDto
        {
            Token = _tokenService.CreateToken(user),
            RefreshToken = refreshToken,
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
