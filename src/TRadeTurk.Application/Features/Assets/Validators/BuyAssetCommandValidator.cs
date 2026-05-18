using FluentValidation;
using TRadeTurk.Application.Common.Interfaces;
using TRadeTurk.Application.Features.Assets.Commands;
using TRadeTurk.Domain.Entities;

namespace TRadeTurk.Application.Features.Assets.Validators;

public class BuyAssetCommandValidator : AbstractValidator<BuyAssetCommand>
{
    private readonly IRepository<Wallet> _walletRepository;
    private const decimal CommissionRate = 0.001m;

    public BuyAssetCommandValidator(IRepository<Wallet> walletRepository)
    {
        _walletRepository = walletRepository;

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanici ID bos olamaz.");

        RuleFor(x => x.Symbol)
            .NotEmpty().WithMessage("Sembol bos olamaz.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Miktar 0'dan buyuk olmalidir.");

        RuleFor(x => x.RequestedPrice)
            .GreaterThan(0).WithMessage("Fiyat 0'dan buyuk olmalidir.");

        RuleFor(x => x)
            .MustAsync(HaveEnoughBalance)
            .WithMessage("Yetersiz bakiye.");
    }

    private async Task<bool> HaveEnoughBalance(BuyAssetCommand command, CancellationToken cancellationToken)
    {
        var wallet = (await _walletRepository.FindAsync(w => w.UserId == command.UserId, cancellationToken)).FirstOrDefault();
        if (wallet == null) return false;

        var estimatedCost = command.Amount * command.RequestedPrice;
        var estimatedCommission = estimatedCost * CommissionRate;

        return wallet.FiatBalance >= estimatedCost + estimatedCommission;
    }
}
