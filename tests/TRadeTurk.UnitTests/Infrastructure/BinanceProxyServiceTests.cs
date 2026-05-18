using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using TRadeTurk.Infrastructure.Services;

namespace TRadeTurk.UnitTests.Infrastructure;

public class BinanceProxyServiceTests
{
    [Fact]
    public async Task GetCurrentPriceAsync_WhenCalledTwiceWithinCacheWindow_ShouldUseCache()
    {
        // Arrange
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var handler = new SequenceHttpMessageHandler(
            HttpResponseMessageFactory.OkPrice("BTCUSDT", "100.50"),
            HttpResponseMessageFactory.OkPrice("BTCUSDT", "200.50"));

        var proxy = CreateProxy(cache, handler);

        // Act
        var first = await proxy.GetCurrentPriceAsync("BTCUSDT", CancellationToken.None);
        var second = await proxy.GetCurrentPriceAsync("BTCUSDT", CancellationToken.None);

        // Assert
        first.Should().Be(100.50m);
        second.Should().Be(100.50m);
        handler.CallCount.Should().Be(1);
    }

    [Fact]
    public async Task GetCurrentPriceAsync_WhenBinanceFailsAndFallbackExists_ShouldReturnFallback()
    {
        // Arrange
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var handler = new SequenceHttpMessageHandler(
            HttpResponseMessageFactory.OkPrice("BTCUSDT", "100.50"),
            new HttpRequestException("Binance unavailable"));

        var proxy = CreateProxy(cache, handler);

        // Act
        var first = await proxy.GetCurrentPriceAsync("BTCUSDT", CancellationToken.None);
        cache.Remove("Binance_Price_BTCUSDT");
        var fallback = await proxy.GetCurrentPriceAsync("BTCUSDT", CancellationToken.None);

        // Assert
        first.Should().Be(100.50m);
        fallback.Should().Be(100.50m);
        handler.CallCount.Should().Be(2);
    }

    private static BinanceProxyService CreateProxy(IMemoryCache cache, HttpMessageHandler handler)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["BinanceApi:BaseUrl"] = "https://example.test/api/v3/"
            })
            .Build();

        var httpClient = new HttpClient(handler);
        var binanceService = new BinanceService(httpClient, configuration, NullLogger<BinanceService>.Instance);

        return new BinanceProxyService(cache, NullLogger<BinanceProxyService>.Instance, binanceService);
    }

    private sealed class SequenceHttpMessageHandler : HttpMessageHandler
    {
        private readonly Queue<object> _responses;

        public SequenceHttpMessageHandler(params object[] responses)
        {
            _responses = new Queue<object>(responses);
        }

        public int CallCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            var next = _responses.Dequeue();

            if (next is Exception exception)
            {
                throw exception;
            }

            return Task.FromResult((HttpResponseMessage)next);
        }
    }

    private static class HttpResponseMessageFactory
    {
        public static HttpResponseMessage OkPrice(string symbol, string price)
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent($"{{\"symbol\":\"{symbol}\",\"price\":\"{price}\"}}", Encoding.UTF8, "application/json")
            };
        }
    }
}
