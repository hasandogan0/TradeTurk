using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TRadeTurk.Infrastructure.Data;

namespace TRadeTurk.IntegrationTests;

public class AuditLogTests
{
    [Fact]
    public async Task Register_ShouldCreateAuditLogEntry()
    {
        await using var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        await client.PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "Audit Test User",
            email = "audit@test.com",
            userName = "audituser",
            password = "Test12345!"
        });

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logs = await context.AuditLogs.ToListAsync();
        logs.Should().Contain(l => l.Action == "Register");
    }

    [Fact]
    public async Task Login_ShouldCreateAuditLogEntry()
    {
        await using var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        await client.PostAsJsonAsync("/api/auth/login", new
        {
            emailOrUserName = "test@example.com",
            password = "Test12345!"
        });

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logs = await context.AuditLogs.ToListAsync();
        logs.Should().Contain(l => l.Action == "Login");
    }

    [Fact]
    public async Task Logout_ShouldCreateAuditLogEntry()
    {
        await using var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        var login = await client.PostAsJsonAsync("/api/auth/login", new
        {
            emailOrUserName = "test@example.com",
            password = "Test12345!"
        });
        var auth = await login.Content.ReadFromJsonAsync<AuthResponse>();

        await client.AuthenticateAsSeededUserAsync();
        await client.PostAsJsonAsync("/api/auth/logout", new { refreshToken = auth!.RefreshToken });

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logs = await context.AuditLogs.ToListAsync();
        logs.Should().Contain(l => l.Action == "Logout");
    }

    [Fact]
    public async Task OrderCreate_ShouldCreateAuditLogEntry()
    {
        await using var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();
        await client.AuthenticateAsSeededUserAsync();

        await client.PostAsJsonAsync("/api/orders", new
        {
            symbol = "BTCUSDT",
            side = "BUY",
            type = "MARKET",
            quantity = 0.01m,
            price = 50000m
        });

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logs = await context.AuditLogs.ToListAsync();
        logs.Should().Contain(l => l.Action.StartsWith("OrderCreate"));
    }

    private sealed class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
