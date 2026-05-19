using MediatR;
using TRadeTurk.Application.DTOs;

namespace TRadeTurk.Application.Features.Assets.Commands;

public class SellAssetCommand : IRequest<TransactionResultDto>
{
    public string Symbol { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal RequestedPrice { get; set; }
}
