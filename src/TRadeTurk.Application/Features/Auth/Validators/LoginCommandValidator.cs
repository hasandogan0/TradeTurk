using FluentValidation;
using TRadeTurk.Application.Features.Auth.Commands;

namespace TRadeTurk.Application.Features.Auth.Validators;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.EmailOrUserName)
            .NotEmpty().WithMessage("Email veya kullanici adi bos olamaz.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Sifre bos olamaz.");
    }
}
