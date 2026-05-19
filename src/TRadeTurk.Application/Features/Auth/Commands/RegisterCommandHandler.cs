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
    private readonly IPasswordHasher _passwordHasher;
    private readonly IVirtualCardFactory _virtualCardFactory;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCommandHandler(
        IRepository<User> userRepository,
        IRepository<Wallet> walletRepository,
        IRepository<Card> cardRepository,
        IPasswordHasher passwordHasher,
        IVirtualCardFactory virtualCardFactory,
        ITokenService tokenService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _walletRepository = walletRepository;
        _cardRepository = cardRepository;
        _passwordHasher = passwordHasher;
        _virtualCardFactory = virtualCardFactory;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
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
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResultDto
        {
            Token = _tokenService.CreateToken(user),
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
