using MediatR;
using TRadeTurk.Application.DTOs;

namespace TRadeTurk.Application.Features.Wallets.Queries;

public class GetMyWalletQuery : IRequest<WalletDetailsDto?>
{
    public Guid UserId { get; set; }
}
