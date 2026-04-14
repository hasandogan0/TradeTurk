using FluentValidation;
using TRadeTurk.Domain.Entities;
using TRadeTurk.Domain.Interfaces;

namespace TRadeTurk.Application.Features.Assets.Validators;

public class BuyAssetCommandValidator : AbstractValidator<BuyAssetCommand>
{
    private readonly IRepository<Wallet> _walletRepository;
    private const decimal CommissionRate = 0.001m;

    public BuyAssetCommandValidator(IRepository<Wallet> walletRepository)
    {
        _walletRepository = walletRepository;

        RuleFor(x => x.WalletId)
            .NotEmpty().WithMessage("Cüzdan ID boş olamaz.");

        RuleFor(x => x.Symbol)
            .NotEmpty().WithMessage("Sembol boş olamaz.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Miktar 0'dan büyük olmalıdır.");

        RuleFor(x => x)
            .MustAsync(HaveEnoughBalance)
            .WithMessage("Yetersiz bakiye (Tahmini tutar + Komisyon bakiyeyi aşıyor).");
    }

    private async Task<bool> HaveEnoughBalance(BuyAssetCommand command, CancellationToken cancellationToken)
    {
        var wallet = await _walletRepository.GetByIdAsync(command.WalletId, cancellationToken);
        if (wallet == null) return false;

        decimal estimatedCost = command.Amount * command.RequestedPrice;
        decimal estimatedCommission = estimatedCost * CommissionRate;
        decimal totalEstimatedDeduct = estimatedCost + estimatedCommission;

        return wallet.FiatBalance >= totalEstimatedDeduct;
    }
}
