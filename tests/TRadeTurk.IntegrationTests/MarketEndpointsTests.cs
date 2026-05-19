using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace TRadeTurk.IntegrationTests;

public class MarketEndpointsTests
{
    [Fact]
    public async Task SymbolsEndpoint_ShouldReturnSupportedMarkets()
    {
        await using var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/markets/symbols");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var symbols = await response.Content.ReadFromJsonAsync<List<string>>();
        symbols.Should().Contain(new[] { "BTCUSDT", "ETHUSDT", "SOLUSDT", "NEARUSDT" });
        symbols.Should().HaveCount(15);
    }

    [Fact]
    public async Task TickersEndpoint_ShouldReturnTickerMetricsWithoutCallingBinance()
    {
        await using var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/markets/tickers");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var tickers = await response.Content.ReadFromJsonAsync<List<TickerResponse>>();
        tickers.Should().Contain(t => t.Symbol == "BTCUSDT" && t.Price == 50000m && t.High24h > t.Low24h);
        tickers.Should().HaveCount(15);
    }

    private sealed class TickerResponse
    {
        public string Symbol { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal High24h { get; set; }
        public decimal Low24h { get; set; }
    }
}
