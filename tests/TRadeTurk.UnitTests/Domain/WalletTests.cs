using FluentAssertions;
using TRadeTurk.Domain.Entities;

namespace TRadeTurk.UnitTests.Domain;

public class WalletTests
{
    [Fact]
    public void Constructor_ShouldRejectNegativeInitialBalance()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var act = () => new Wallet(userId, -1m);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void DeductFiat_WhenBalanceIsEnough_ShouldDecreaseBalance()
    {
        // Arrange
        var wallet = new Wallet(Guid.NewGuid(), 1000.50m);

        // Act
        wallet.DeductFiat(250.25m);

        // Assert
        wallet.FiatBalance.Should().Be(750.25m);
    }

    [Fact]
    public void DeductFiat_WhenBalanceIsInsufficient_ShouldRejectTransaction()
    {
        // Arrange
        var wallet = new Wallet(Guid.NewGuid(), 100m);

        // Act
        var act = () => wallet.DeductFiat(100.01m);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }
}
