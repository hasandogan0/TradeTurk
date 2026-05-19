using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TRadeTurk.Application.Common;
using TRadeTurk.Application.Common.Interfaces;
using TRadeTurk.Infrastructure.Hubs;

namespace TRadeTurk.Infrastructure.BackgroundJobs;

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
        _logger.LogInformation("Binance Data Worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var marketData = scope.ServiceProvider.GetRequiredService<IMarketDataService>();
                var tickers = await marketData.GetTickersAsync(MarketSymbols.Supported, stoppingToken);

                foreach (var ticker in tickers)
                {
                    _logger.LogInformation("Worker retrieved current price for {Symbol}: {Price}", ticker.Symbol, ticker.Price);
                    await _hubContext.Clients.All.SendAsync("ReceivePriceUpdate", ticker.Symbol, ticker.Price, stoppingToken);
                    await _hubContext.Clients.All.SendAsync("ReceiveTickerUpdate", ticker, stoppingToken);
                }

                var delayMinutes = new Random().Next(2, 4);
                _logger.LogInformation("Worker resting for {DelayMinutes} minutes to respect API rate limits...", delayMinutes);
                await Task.Delay(TimeSpan.FromMinutes(delayMinutes), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in Binance Data Worker while trying to fetch market data.");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        _logger.LogInformation("Binance Data Worker shutting down.");
    }
}
