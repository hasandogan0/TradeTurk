using MediatR;
using TRadeTurk.Application.DTOs;

namespace TRadeTurk.Application.Features.Assets.Commands;

public class SellAssetCommand : IRequest<TransactionResultDto>
{
    public Guid WalletId { get; set; }
    public string Symbol { get; set; } = string.Empty; 
    public decimal Amount { get; set; } // Satılacak miktar
    public decimal RequestedPrice { get; set; } // Kullanıcının ekranda gördüğü anlık fiyat
}
