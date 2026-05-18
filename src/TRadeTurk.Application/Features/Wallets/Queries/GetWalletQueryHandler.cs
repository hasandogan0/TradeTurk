using MediatR;
using TRadeTurk.Application.Common.Interfaces;
using TRadeTurk.Application.DTOs;
using TRadeTurk.Domain.Entities;

namespace TRadeTurk.Application.Features.Wallets.Queries;

public class GetWalletQueryHandler : IRequestHandler<GetWalletQuery, WalletDto?>
{
    private readonly IRepository<Wallet> _walletRepository;

    public GetWalletQueryHandler(IRepository<Wallet> walletRepository)
    {
        _walletRepository = walletRepository;
    }

    public async Task<WalletDto?> Handle(GetWalletQuery request, CancellationToken cancellationToken)
    {
        var wallet = (await _walletRepository.FindAsync(w => w.UserId == request.UserId, cancellationToken)).FirstOrDefault();
        if (wallet == null) return null;

        return new WalletDto
        {
            Id = wallet.Id,
            UserId = wallet.UserId,
            FiatBalance = wallet.FiatBalance
        };
    }
}
