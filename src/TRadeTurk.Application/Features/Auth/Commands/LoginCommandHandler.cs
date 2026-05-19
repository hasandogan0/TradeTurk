using MediatR;
using TRadeTurk.Application.Common.Interfaces;
using TRadeTurk.Application.DTOs;
using TRadeTurk.Domain.Entities;

namespace TRadeTurk.Application.Features.Auth.Commands;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResultDto>
{
    private readonly IRepository<User> _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(IRepository<User> userRepository, IPasswordHasher passwordHasher, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<AuthResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var key = request.EmailOrUserName.Trim().ToLowerInvariant();
        var user = (await _userRepository.FindAsync(u => u.Email == key || u.UserName.ToLower() == key, cancellationToken)).FirstOrDefault();

        if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new InvalidOperationException("Email/kullanici adi veya sifre hatali.");
        }

        return new AuthResultDto
        {
            Token = _tokenService.CreateToken(user),
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
