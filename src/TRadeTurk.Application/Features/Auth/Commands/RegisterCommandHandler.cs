using MediatR;
using TRadeTurk.Application.Common.Interfaces;
using TRadeTurk.Application.DTOs;
using TRadeTurk.Domain.Entities;

namespace TRadeTurk.Application.Features.Auth.Commands;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResultDto>
{
    private const decimal DemoBalance = 50000m;

    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Wallet> _walletRepository;
    private readonly IRepository<Card> _cardRepository;
    private readonly IRepository<RefreshToken> _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IVirtualCardFactory _virtualCardFactory;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public RegisterCommandHandler(
        IRepository<User> userRepository,
        IRepository<Wallet> walletRepository,
        IRepository<Card> cardRepository,
        IRepository<RefreshToken> refreshTokenRepository,
        IPasswordHasher passwordHasher,
        IVirtualCardFactory virtualCardFactory,
        ITokenService tokenService,
        IRefreshTokenService refreshTokenService,
        IUnitOfWork unitOfWork,
        IAuditService auditService)
    {
        _userRepository = userRepository;
        _walletRepository = walletRepository;
        _cardRepository = cardRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _virtualCardFactory = virtualCardFactory;
        _tokenService = tokenService;
        _refreshTokenService = refreshTokenService;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<AuthResultDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var userName = request.UserName.Trim();
        var existing = await _userRepository.FindAsync(u => u.Email == email || u.UserName == userName, cancellationToken);
        if (existing.Any())
        {
            throw new InvalidOperationException("Bu email veya kullanici adi zaten kullaniliyor.");
        }

        var user = new User(request.FullName, email, userName, _passwordHasher.Hash(request.Password));
        var wallet = new Wallet(user.Id, DemoBalance);
        var card = _virtualCardFactory.Create(user.Id, wallet.Id, user.FullName, DemoBalance);

        await _userRepository.AddAsync(user, cancellationToken);
        await _walletRepository.AddAsync(wallet, cancellationToken);
        await _cardRepository.AddAsync(card, cancellationToken);
        var refreshToken = _refreshTokenService.CreateRefreshToken();
        await _refreshTokenRepository.AddAsync(new RefreshToken(user.Id, _refreshTokenService.Hash(refreshToken), _refreshTokenService.GetExpiry()), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync(user.Id, "Register", cancellationToken);

        return new AuthResultDto
        {
            Token = _tokenService.CreateToken(user),
            RefreshToken = refreshToken,
            User = ToDto(user)
        };
    }

    private static UserDto ToDto(User user) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        Email = user.Email,
        UserName = user.UserName,
        PreferredCurrency = user.PreferredCurrency,
        ThemePreference = user.ThemePreference
    };
}
