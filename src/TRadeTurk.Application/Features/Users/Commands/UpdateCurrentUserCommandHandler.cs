using MediatR;
using TRadeTurk.Application.Common.Interfaces;
using TRadeTurk.Application.DTOs;
using TRadeTurk.Domain.Entities;

namespace TRadeTurk.Application.Features.Users.Commands;

public class UpdateCurrentUserCommandHandler : IRequestHandler<UpdateCurrentUserCommand, UserDto?>
{
    private readonly IRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCurrentUserCommandHandler(IRepository<User> userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserDto?> Handle(UpdateCurrentUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null) return null;

        var email = request.Email.Trim().ToLowerInvariant();
        var userName = request.UserName.Trim();
        var duplicates = await _userRepository.FindAsync(
            u => u.Id != request.UserId && (u.Email == email || u.UserName == userName),
            cancellationToken);
        if (duplicates.Any())
        {
            throw new InvalidOperationException("Bu email veya kullanici adi zaten kullaniliyor.");
        }

        user.UpdateProfile(request.FullName, email, userName, request.PreferredCurrency, request.ThemePreference);
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            UserName = user.UserName,
            PreferredCurrency = user.PreferredCurrency,
            ThemePreference = user.ThemePreference
        };
    }
}
