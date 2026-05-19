using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TRadeTurk.Application.Features.Portfolio.Commands;

namespace TRadeTurk.Infrastructure.BackgroundJobs;

public class PortfolioSnapshotWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PortfolioSnapshotWorker> _logger;

    public PortfolioSnapshotWorker(IServiceScopeFactory scopeFactory, ILogger<PortfolioSnapshotWorker> logger)
    {
        _scopeFactory = scopeFactory;
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
                var created = await mediator.Send(new CreatePortfolioSnapshotsCommand(), stoppingToken);
                _logger.LogInformation("Created {Count} portfolio snapshots.", created);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Portfolio snapshot worker failed.");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
