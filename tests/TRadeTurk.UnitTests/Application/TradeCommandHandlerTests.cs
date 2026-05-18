using FluentAssertions;
using Moq;
using TRadeTurk.Application.Common.Interfaces;
using TRadeTurk.Application.Features.Assets.Commands;
using TRadeTurk.Domain.Entities;
using TRadeTurk.UnitTests.TestDoubles;

namespace TRadeTurk.UnitTests.Application;

public class TradeCommandHandlerTests
{
    [Fact]
    public async Task BuyAssetCommand_WhenBalanceIsEnough_ShouldCreateAssetAndTransaction()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var wallet = new Wallet(userId, 10000m);
        var walletRepository = new InMemoryRepository<Wallet>(wallet);
        var assetRepository = new InMemoryRepository<Asset>();
        var transactionRepository = new InMemoryRepository<Transaction>();
        var priceProvider = new Mock<IPriceProviderContext>();
        priceProvider.Setup(x => x.GetCurrentPriceAsync("BTCUSDT", It.IsAny<CancellationToken>()))
            .ReturnsAsync(100m);

        var handler = new BuyAssetCommandHandler(
            walletRepository,
            assetRepository,
            transactionRepository,
            new NoOpUnitOfWork(),
            priceProvider.Object);

        // Act
        var result = await handler.Handle(new BuyAssetCommand
        {
            UserId = userId,
            Symbol = "BTCUSDT",
            Amount = 1m,
            RequestedPrice = 100m
        }, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        wallet.FiatBalance.Should().BeLessThan(9900m);
        assetRepository.Items.Should().ContainSingle(a => a.Symbol == "BTCUSDT" && a.Amount == 1m);
        transactionRepository.Items.Should().ContainSingle();
    }

    [Fact]
    public async Task BuyAssetCommand_WhenBalanceIsInsufficient_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var walletRepository = new InMemoryRepository<Wallet>(new Wallet(userId, 1m));
        var priceProvider = new Mock<IPriceProviderContext>();
        priceProvider.Setup(x => x.GetCurrentPriceAsync("BTCUSDT", It.IsAny<CancellationToken>()))
            .ReturnsAsync(100m);

        var handler = new BuyAssetCommandHandler(
            walletRepository,
            new InMemoryRepository<Asset>(),
            new InMemoryRepository<Transaction>(),
            new NoOpUnitOfWork(),
            priceProvider.Object);

        // Act
        var result = await handler.Handle(new BuyAssetCommand
        {
            UserId = userId,
            Symbol = "BTCUSDT",
            Amount = 1m,
            RequestedPrice = 100m
        }, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Yetersiz");
    }

    [Fact]
    public async Task SellAssetCommand_WhenAssetIsEnough_ShouldDecreaseAssetAndIncreaseWallet()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var wallet = new Wallet(userId, 1000m);
        var asset = new Asset(userId, wallet.Id, "ETHUSDT");
        asset.AddAmount(2m, 2000m);

        var transactionRepository = new InMemoryRepository<Transaction>();
        var priceProvider = new Mock<IPriceProviderContext>();
        priceProvider.Setup(x => x.GetCurrentPriceAsync("ETHUSDT", It.IsAny<CancellationToken>()))
            .ReturnsAsync(2000m);

        var handler = new SellAssetCommandHandler(
            new InMemoryRepository<Wallet>(wallet),
            new InMemoryRepository<Asset>(asset),
            transactionRepository,
            new NoOpUnitOfWork(),
            priceProvider.Object);

        // Act
        var result = await handler.Handle(new SellAssetCommand
        {
            UserId = userId,
            Symbol = "ETHUSDT",
            Amount = 1m,
            RequestedPrice = 2000m
        }, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        asset.Amount.Should().Be(1m);
        wallet.FiatBalance.Should().BeGreaterThan(1000m);
        transactionRepository.Items.Should().ContainSingle();
    }

    [Fact]
    public async Task SellAssetCommand_WhenAssetIsInsufficient_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var wallet = new Wallet(userId, 1000m);
        var asset = new Asset(userId, wallet.Id, "ETHUSDT");
        asset.AddAmount(0.5m, 2000m);

        var handler = new SellAssetCommandHandler(
            new InMemoryRepository<Wallet>(wallet),
            new InMemoryRepository<Asset>(asset),
            new InMemoryRepository<Transaction>(),
            new NoOpUnitOfWork(),
            Mock.Of<IPriceProviderContext>());

        // Act
        var result = await handler.Handle(new SellAssetCommand
        {
            UserId = userId,
            Symbol = "ETHUSDT",
            Amount = 1m,
            RequestedPrice = 2000m
        }, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Yetersiz");
    }
}
