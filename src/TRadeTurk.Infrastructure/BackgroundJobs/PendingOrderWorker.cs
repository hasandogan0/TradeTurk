using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TRadeTurk.Application.Features.Orders.Commands;
using TRadeTurk.Infrastructure.Hubs;

namespace TRadeTurk.Infrastructure.BackgroundJobs;

public class PendingOrderWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<PriceHub> _hubContext;
    private readonly ILogger<PendingOrderWorker> _logger;

    public PendingOrderWorker(IServiceScopeFactory scopeFactory, IHubContext<PriceHub> hubContext, ILogger<PendingOrderWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                var executed = await mediator.Send(new ProcessPendingOrdersCommand(), stoppingToken);
                if (executed > 0)
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveNotification", new
                    {
                        type = "order",
                        message = $"{executed} bekleyen emir gerceklesti.",
                        createdAt = DateTime.UtcNow
                    }, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Pending order worker failed.");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
