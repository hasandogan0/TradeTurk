using Microsoft.AspNetCore.SignalR;

namespace TRadeTurk.Infrastructure.Hubs;

public class PriceHub : Hub
{
    // Hub metotları buraya eklenebilir. Şimdilik sadece yayın (broadcast) yapılacak.
    public async Task SendPriceUpdate(string symbol, decimal price)
    {
        await Clients.All.SendAsync("ReceivePriceUpdate", symbol, price);
    }
}
