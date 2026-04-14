using FluentValidation;
using TRadeTurk.Domain.Entities;
using TRadeTurk.Domain.Interfaces;

namespace TRadeTurk.Application.Features.Assets.Validators;

public class SellAssetCommandValidator : AbstractValidator<SellAssetCommand>
{
    private readonly IRepository<Asset> _assetRepository;

    public SellAssetCommandValidator(IRepository<Asset> assetRepository)
    {
        _assetRepository = assetRepository;

        RuleFor(x => x.WalletId)
            .NotEmpty().WithMessage("Cüzdan ID boş olamaz.");

        RuleFor(x => x.Symbol)
            .NotEmpty().WithMessage("Sembol boş olamaz.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Miktar 0'dan büyük olmalıdır.");

        RuleFor(x => x)
            .MustAsync(HaveEnoughAsset)
            .WithMessage("Yetersiz kripto varlık miktarı.");
    }

    private async Task<bool> HaveEnoughAsset(SellAssetCommand command, CancellationToken cancellationToken)
    {
        var existingAssets = await _assetRepository.FindAsync(a => a.WalletId == command.WalletId && a.Symbol == command.Symbol, cancellationToken);
        var asset = existingAssets.FirstOrDefault();

        return asset != null && asset.Amount >= command.Amount;
    }
}
