using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace TRadeTurk.IntegrationTests;

public class TradeEndpointsTests
{
    [Fact]
    public async Task BuyEndpoint_WhenRequestIsValid_ShouldReturnSuccess()
    {
        // Arrange
        await using var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/trade/buy", new
        {
            userId = TestWebApplicationFactory.TestUserId,
            symbol = "BTCUSDT",
            amount = 0.1m,
            requestedPrice = 50000m
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<TradeResultResponse>();
        json!.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SellEndpoint_WhenRequestIsValid_ShouldReturnSuccess()
    {
        // Arrange
        await using var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/trade/sell", new
        {
            userId = TestWebApplicationFactory.TestUserId,
            symbol = "BTCUSDT",
            amount = 0.25m,
            requestedPrice = 50000m
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<TradeResultResponse>();
        json!.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task BuyEndpoint_WhenRequestIsInvalid_ShouldReturnBadRequest()
    {
        // Arrange
        await using var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/trade/buy", new
        {
            userId = TestWebApplicationFactory.TestUserId,
            symbol = "",
            amount = 0m,
            requestedPrice = 50000m
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private sealed class TradeResultResponse
    {
        public bool IsSuccess { get; set; }
    }
}
