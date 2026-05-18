using FluentAssertions;
using Moq;
using TRadeTurk.Application.Common.Interfaces;
using TRadeTurk.Infrastructure.Services;

namespace TRadeTurk.UnitTests.Infrastructure;

public class PriceProviderTests
{
    [Fact]
    public async Task BinancePriceProviderStrategy_ShouldCallBinancePriceService()
    {
        // Arrange
        var binanceService = new Mock<IBinancePriceService>();
        binanceService.Setup(x => x.GetCurrentPriceAsync("BTCUSDT", It.IsAny<CancellationToken>()))
            .ReturnsAsync(123m);

        var strategy = new BinancePriceProviderStrategy(binanceService.Object);

        // Act
        var price = await strategy.GetCurrentPriceAsync("BTCUSDT", CancellationToken.None);

        // Assert
        price.Should().Be(123m);
        binanceService.Verify(x => x.GetCurrentPriceAsync("BTCUSDT", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MockPriceProviderStrategy_ShouldReturnDeterministicDemoPrice()
    {
        // Arrange
        var strategy = new MockPriceProviderStrategy();

        // Act
        var first = await strategy.GetCurrentPriceAsync("BTCUSDT", CancellationToken.None);
        var second = await strategy.GetCurrentPriceAsync("BTCUSDT", CancellationToken.None);

        // Assert
        first.Should().Be(65000m);
        second.Should().Be(first);
    }
}
