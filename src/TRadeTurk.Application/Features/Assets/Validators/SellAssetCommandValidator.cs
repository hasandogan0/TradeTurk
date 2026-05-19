using FluentValidation;
using TRadeTurk.Application.Features.Assets.Commands;

namespace TRadeTurk.Application.Features.Assets.Validators;

public class SellAssetCommandValidator : AbstractValidator<SellAssetCommand>
{
    public SellAssetCommandValidator()
    {
        RuleFor(x => x.Symbol)
            .NotEmpty().WithMessage("Sembol bos olamaz.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Miktar 0'dan buyuk olmalidir.");

        RuleFor(x => x.RequestedPrice)
            .GreaterThan(0).WithMessage("Fiyat 0'dan buyuk olmalidir.");
    }
}
