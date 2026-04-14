using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using TRadeTurk.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;
using TRadeTurk.Infrastructure.Hubs;

namespace TRadeTurk.Infrastructure.BackgroundJobs;

/// <summary>
/// 2-3 dakikada bir Binance'ten veri çeken Background Service (Worker)
/// </summary>
public class BinanceDataWorker : BackgroundService
{
    private readonly ILogger<BinanceDataWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IHubContext<PriceHub> _hubContext;

    public BinanceDataWorker(ILogger<BinanceDataWorker> logger, IServiceProvider serviceProvider, IHubContext<PriceHub> hubContext)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Binance Data Worker started. Simulating periodic external data fetching.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var binanceService = scope.ServiceProvider.GetRequiredService<IBinanceService>();

                var symbolsToTrack = new[] { "BTCUSDT", "ETHUSDT" };

                foreach (var symbol in symbolsToTrack)
                {
                    decimal price = await binanceService.GetCurrentPriceAsync(symbol, stoppingToken);
                    _logger.LogInformation("Worker retrieved current price for {Symbol}: {Price}", symbol, price);
                    
                    // SignalR Hub üzerinden tüm istemcilere fiyatı duyur
                    await _hubContext.Clients.All.SendAsync("ReceivePriceUpdate", symbol, price, stoppingToken);

                    // TODO: Mediator/CQRS entegrasyonu ile fiyat değişiklikleri Handle edilmeli.
                    // var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                    // await mediator.Publish(new PriceUpdatedEvent(symbol, price));
                }

                // API hız limitleri (Rate Limiting) simülasyonu için 2 ile 3 dakika arasında bekleme
                int delayMinutes = new Random().Next(2, 4); 
                _logger.LogInformation("Worker resting for {DelayMinutes} minutes to respect API rate limits...", delayMinutes);
                await Task.Delay(TimeSpan.FromMinutes(delayMinutes), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in Binance Data Worker while trying to fetch data.");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
        
        _logger.LogInformation("Binance Data Worker shutting down.");
    }
}
