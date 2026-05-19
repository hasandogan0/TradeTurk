using FluentValidation;
using TRadeTurk.Application.Features.Auth.Commands;

namespace TRadeTurk.Application.Features.Auth.Validators;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Ad soyad bos olamaz.")
            .MinimumLength(2).WithMessage("Ad soyad en az 2 karakter olmalidir.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email bos olamaz.")
            .EmailAddress().WithMessage("Gecerli bir email girin.");

        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Kullanici adi bos olamaz.")
            .MinimumLength(3).WithMessage("Kullanici adi en az 3 karakter olmalidir.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Sifre bos olamaz.")
            .MinimumLength(8).WithMessage("Sifre en az 8 karakter olmalidir.");
    }
}
