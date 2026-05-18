using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace TRadeTurk.IntegrationTests;

public class PortfolioEndpointsTests
{
    [Fact]
    public async Task GetWalletEndpoint_ShouldReturnSeededWallet()
    {
        // Arrange
        await using var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/wallet/{TestWebApplicationFactory.TestUserId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var wallet = await response.Content.ReadFromJsonAsync<WalletResponse>();
        wallet!.UserId.Should().Be(TestWebApplicationFactory.TestUserId);
        wallet.FiatBalance.Should().Be(100000m);
    }

    [Fact]
    public async Task GetAssetsEndpoint_ShouldReturnSeededAssets()
    {
        // Arrange
        await using var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/assets/{TestWebApplicationFactory.TestUserId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var assets = await response.Content.ReadFromJsonAsync<List<AssetResponse>>();
        assets.Should().ContainSingle(a => a.Symbol == "BTCUSDT" && a.Amount == 2m);
    }

    private sealed class WalletResponse
    {
        public Guid UserId { get; set; }
        public decimal FiatBalance { get; set; }
    }

    private sealed class AssetResponse
    {
        public string Symbol { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
