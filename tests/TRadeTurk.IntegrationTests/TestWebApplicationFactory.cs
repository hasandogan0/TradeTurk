using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TRadeTurk.Application.Common.Interfaces;
using TRadeTurk.Domain.Entities;
using TRadeTurk.Infrastructure.Data;
using TRadeTurk.Infrastructure.Services;

namespace TRadeTurk.IntegrationTests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    public static readonly Guid TestUserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(logging => logging.ClearProviders());

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
            services.RemoveAll<IMarketDataService>();

            _connection.Open();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(_connection));

            services.AddScoped<IPriceProviderContext, FixedPriceProviderContext>();
            services.AddScoped<IMarketDataService, FixedMarketDataService>();

            using var scope = services.BuildServiceProvider().CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.Database.EnsureCreated();

            var passwordHasher = new PasswordHasher();
            var user = new User("Test User", "test@example.com", "testuser", passwordHasher.Hash("Test12345!"));
            var wallet = new Wallet(TestUserId, 100000m);
            var asset = new Asset(TestUserId, wallet.Id, "BTCUSDT");
            asset.AddAmount(2m, 50000m);

            typeof(TRadeTurk.Domain.Common.BaseEntity)
                .GetProperty(nameof(TRadeTurk.Domain.Common.BaseEntity.Id))!
                .SetValue(user, TestUserId);

            context.Users.Add(user);
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

    private sealed class FixedMarketDataService : IMarketDataService
    {
        public Task<TRadeTurk.Application.DTOs.MarketTickerDto> GetTickerAsync(string symbol, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Create(symbol));
        }

        public Task<IReadOnlyCollection<TRadeTurk.Application.DTOs.MarketTickerDto>> GetTickersAsync(IReadOnlyCollection<string> symbols, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<TRadeTurk.Application.DTOs.MarketTickerDto>>(symbols.Select(Create).ToArray());
        }

        private static TRadeTurk.Application.DTOs.MarketTickerDto Create(string symbol)
        {
            var price = symbol.Trim().ToUpperInvariant() switch
            {
                "BTCUSDT" => 50000m,
                "ETHUSDT" => 3000m,
                "SOLUSDT" => 150m,
                _ => 100m
            };

            return new TRadeTurk.Application.DTOs.MarketTickerDto
            {
                Symbol = symbol.Trim().ToUpperInvariant(),
                Price = price,
                ChangePercent24h = 2.5m,
                High24h = price * 1.05m,
                Low24h = price * 0.95m,
                Volume24h = 123456m,
                RetrievedAtUtc = DateTime.UtcNow
            };
        }
    }
}
