using MediatR;
using TRadeTurk.Application.DTOs;

namespace TRadeTurk.Application.Features.Assets.Commands;

public class BuyAssetCommand : IRequest<TransactionResultDto>
{
    public Guid WalletId { get; set; }
    public string Symbol { get; set; } = string.Empty; // Örn: BTCUSDT
    public decimal Amount { get; set; } // Alınacak miktar
    public decimal RequestedPrice { get; set; } // Kullanıcının ekranda gördüğü anlık fiyat
}
