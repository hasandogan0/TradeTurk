using MediatR;
using TRadeTurk.Application.Common.Interfaces;
using TRadeTurk.Domain.Entities;

namespace TRadeTurk.Application.Features.Users.Commands;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, bool>
{
    private readonly IRepository<User> _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public ChangePasswordCommandHandler(IRepository<User> userRepository, IPasswordHasher passwordHasher, IUnitOfWork unitOfWork, IAuditService auditService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<bool> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null) return false;
        if (!_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
        {
            throw new InvalidOperationException("Mevcut sifre hatali.");
        }

        user.ChangePasswordHash(_passwordHasher.Hash(request.NewPassword));
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync(user.Id, "PasswordChange", cancellationToken);
        return true;
    }
}
