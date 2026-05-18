using FluentAssertions;
using TRadeTurk.Domain.Entities;

namespace TRadeTurk.UnitTests.Domain;

public class AssetTests
{
    [Fact]
    public void DeductAmount_WhenAssetIsEnough_ShouldDecreaseAmount()
    {
        // Arrange
        var asset = new Asset(Guid.NewGuid(), Guid.NewGuid(), "BTCUSDT");
        asset.AddAmount(2.5m, 50000m);

        // Act
        asset.DeductAmount(1.25m);

        // Assert
        asset.Amount.Should().Be(1.25m);
    }

    [Fact]
    public void DeductAmount_WhenAssetIsInsufficient_ShouldRejectSale()
    {
        // Arrange
        var asset = new Asset(Guid.NewGuid(), Guid.NewGuid(), "ETHUSDT");
        asset.AddAmount(1m, 3000m);

        // Act
        var act = () => asset.DeductAmount(1.00000001m);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddAmount_ShouldPreserveDecimalPrecisionForAverageCost()
    {
        // Arrange
        var asset = new Asset(Guid.NewGuid(), Guid.NewGuid(), "BTCUSDT");

        // Act
        asset.AddAmount(0.1m, 100.10m);
        asset.AddAmount(0.2m, 200.20m);

        // Assert
        asset.Amount.Should().Be(0.3m);
        asset.AverageCost.Should().BeApproximately(166.83333333333333333333333333m, 0.00000000000000000000000001m);
    }
}
