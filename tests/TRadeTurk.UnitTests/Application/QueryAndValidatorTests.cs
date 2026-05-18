using FluentAssertions;
using TRadeTurk.Application.Features.Assets.Commands;
using TRadeTurk.Application.Features.Assets.Validators;
using TRadeTurk.Application.Features.Wallets.Queries;
using TRadeTurk.Domain.Entities;
using TRadeTurk.UnitTests.TestDoubles;

namespace TRadeTurk.UnitTests.Application;

public class QueryAndValidatorTests
{
    [Fact]
    public async Task GetWalletQuery_ShouldReturnUserWallet()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var wallet = new Wallet(userId, 1234.56m);
        var handler = new GetWalletQueryHandler(new InMemoryRepository<Wallet>(wallet));

        // Act
        var result = await handler.Handle(new GetWalletQuery { UserId = userId }, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
        result.FiatBalance.Should().Be(1234.56m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task BuyAssetCommandValidator_WhenAmountIsNotPositive_ShouldReturnError(decimal amount)
    {
        // Arrange
        var validator = new BuyAssetCommandValidator(new InMemoryRepository<Wallet>());

        // Act
        var result = await validator.ValidateAsync(new BuyAssetCommand
        {
            UserId = Guid.NewGuid(),
            Symbol = "BTCUSDT",
            Amount = amount,
            RequestedPrice = 1m
        });

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(BuyAssetCommand.Amount));
    }

    [Fact]
    public async Task BuyAssetCommandValidator_WhenSymbolIsEmpty_ShouldReturnError()
    {
        // Arrange
        var validator = new BuyAssetCommandValidator(new InMemoryRepository<Wallet>());

        // Act
        var result = await validator.ValidateAsync(new BuyAssetCommand
        {
            UserId = Guid.NewGuid(),
            Symbol = "",
            Amount = 1m,
            RequestedPrice = 1m
        });

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(BuyAssetCommand.Symbol));
    }

    [Fact]
    public async Task SellAssetCommandValidator_WhenUserIdIsEmpty_ShouldReturnError()
    {
        // Arrange
        var validator = new SellAssetCommandValidator(new InMemoryRepository<Asset>());

        // Act
        var result = await validator.ValidateAsync(new SellAssetCommand
        {
            UserId = Guid.Empty,
            Symbol = "BTCUSDT",
            Amount = 1m,
            RequestedPrice = 1m
        });

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SellAssetCommand.UserId));
    }
}
