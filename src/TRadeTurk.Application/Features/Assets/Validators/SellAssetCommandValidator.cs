using FluentValidation;
using TRadeTurk.Application.Common.Interfaces;
using TRadeTurk.Application.Features.Assets.Commands;
using TRadeTurk.Domain.Entities;

namespace TRadeTurk.Application.Features.Assets.Validators;

public class SellAssetCommandValidator : AbstractValidator<SellAssetCommand>
{
    private readonly IRepository<Asset> _assetRepository;

    public SellAssetCommandValidator(IRepository<Asset> assetRepository)
    {
        _assetRepository = assetRepository;

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanici ID bos olamaz.");

        RuleFor(x => x.Symbol)
            .NotEmpty().WithMessage("Sembol bos olamaz.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Miktar 0'dan buyuk olmalidir.");

        RuleFor(x => x.RequestedPrice)
            .GreaterThan(0).WithMessage("Fiyat 0'dan buyuk olmalidir.");

        RuleFor(x => x)
            .MustAsync(HaveEnoughAsset)
            .WithMessage("Yetersiz kripto varlik miktari.");
    }

    private async Task<bool> HaveEnoughAsset(SellAssetCommand command, CancellationToken cancellationToken)
    {
        var symbol = command.Symbol.Trim().ToUpperInvariant();
        var asset = (await _assetRepository.FindAsync(
            a => a.UserId == command.UserId && a.Symbol == symbol,
            cancellationToken)).FirstOrDefault();

        return asset != null && asset.Amount >= command.Amount;
    }
}
