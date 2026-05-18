using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using TRadeTurk.Application.Common.Interfaces;
using TRadeTurk.Domain.Entities;
using TRadeTurk.Infrastructure.Data;

namespace TRadeTurk.IntegrationTests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    public static readonly Guid TestUserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            foreach (var descriptor in services
                .Where(d => d.ServiceType == typeof(ApplicationDbContext)
                    || d.ServiceType == typeof(DbContextOptions)
                    || d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>)
                    || d.ServiceType.Name.Contains("DbContextOptions", StringComparison.Ordinal))
                .ToList())
            {
                services.Remove(descriptor);
            }

            services.RemoveAll<IHostedService>();
            services.RemoveAll<IPriceProviderContext>();

            _connection.Open();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(_connection));

            services.AddScoped<IPriceProviderContext, FixedPriceProviderContext>();

            using var scope = services.BuildServiceProvider().CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.Database.EnsureCreated();

            var wallet = new Wallet(TestUserId, 100000m);
            var asset = new Asset(TestUserId, wallet.Id, "BTCUSDT");
            asset.AddAmount(2m, 50000m);

            context.Wallets.Add(wallet);
            context.Assets.Add(asset);
            context.SaveChanges();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _connection.Dispose();
        }
    }

    private sealed class FixedPriceProviderContext : IPriceProviderContext
    {
        public Task<decimal> GetCurrentPriceAsync(string symbol, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(symbol.Trim().ToUpperInvariant() switch
            {
                "BTCUSDT" => 50000m,
                "ETHUSDT" => 3000m,
                _ => 100m
            });
        }
    }
}
