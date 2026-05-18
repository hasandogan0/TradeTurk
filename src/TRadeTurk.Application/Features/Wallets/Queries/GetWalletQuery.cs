using MediatR;
using TRadeTurk.Application.DTOs;

namespace TRadeTurk.Application.Features.Wallets.Queries;

public class GetWalletQuery : IRequest<WalletDto?>
{
    public Guid UserId { get; set; }
}
