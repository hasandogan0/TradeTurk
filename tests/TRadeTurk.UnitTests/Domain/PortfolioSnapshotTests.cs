using FluentAssertions;
using TRadeTurk.Domain.Entities;

namespace TRadeTurk.UnitTests.Domain;

public class PortfolioSnapshotTests
{
    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        var userId = Guid.NewGuid();
        var snapshot = new PortfolioSnapshot(userId, 55000m, 50000m, 5000m, 200m);

        snapshot.UserId.Should().Be(userId);
        snapshot.TotalValue.Should().Be(55000m);
        snapshot.AvailableUSDT.Should().Be(50000m);
        snapshot.AssetValue.Should().Be(5000m);
        snapshot.TotalPnL.Should().Be(200m);
    }

    [Fact]
    public void Constructor_ShouldRejectEmptyUserId()
    {
        var act = () => new PortfolioSnapshot(Guid.Empty, 55000m, 50000m, 5000m, 200m);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldAcceptNegativePnL()
    {
        var snapshot = new PortfolioSnapshot(Guid.NewGuid(), 48000m, 40000m, 8000m, -2000m);

        snapshot.TotalPnL.Should().Be(-2000m);
    }
}
