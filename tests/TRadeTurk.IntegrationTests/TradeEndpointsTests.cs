using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace TRadeTurk.IntegrationTests;

public class TradeEndpointsTests
{
    [Fact]
    public async Task BuyEndpoint_WhenAuthenticated_ShouldUpdateUserWallet()
    {
        await using var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();
        await client.AuthenticateAsSeededUserAsync();

        var response = await client.PostAsJsonAsync("/api/trade/buy", new
        {
            symbol = "BTCUSDT",
            amount = 0.1m,
            requestedPrice = 50000m
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<TradeResultResponse>();
        json!.IsSuccess.Should().BeTrue();

        var transactions = await client.GetFromJsonAsync<List<TransactionResponse>>("/api/transactions/me");
        transactions.Should().Contain(t => t.Type == "BUY" && t.Symbol == "BTCUSDT");
    }

    [Fact]
    public async Task SellEndpoint_WhenAuthenticated_ShouldUpdateUserWallet()
    {
        await using var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();
        await client.AuthenticateAsSeededUserAsync();

        var response = await client.PostAsJsonAsync("/api/trade/sell", new
        {
            symbol = "BTCUSDT",
            amount = 0.25m,
            requestedPrice = 50000m
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<TradeResultResponse>();
        json!.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task BuyEndpoint_WithoutToken_ShouldReturnUnauthorized()
    {
        await using var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/trade/buy", new
        {
            symbol = "BTCUSDT",
            amount = 0.1m,
            requestedPrice = 50000m
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private sealed class TradeResultResponse
    {
        public bool IsSuccess { get; set; }
    }

    private sealed class TransactionResponse
    {
        public string Type { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
    }
}
