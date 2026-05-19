using FluentAssertions;
using TRadeTurk.Domain.Entities;
using TRadeTurk.Domain.Enums;

namespace TRadeTurk.UnitTests.Domain;

public class OrderTests
{
    [Fact]
    public void MarketOrder_ShouldBeFilledOnCreation()
    {
        var order = new Order(Guid.NewGuid(), "BTCUSDT", OrderSide.Buy, OrderType.Market, 0.5m, 50000m, null);
        order.Status.Should().Be(OrderStatus.Filled);
    }

    [Fact]
    public void LimitOrder_ShouldBePendingOnCreation()
    {
        var order = new Order(Guid.NewGuid(), "BTCUSDT", OrderSide.Buy, OrderType.Limit, 0.5m, 50000m, null);
        order.Status.Should().Be(OrderStatus.Pending);
    }

    [Fact]
    public void StopLossOrder_ShouldBePendingOnCreation()
    {
        var order = new Order(Guid.NewGuid(), "BTCUSDT", OrderSide.Sell, OrderType.StopLoss, 0.5m, null, 48000m);
        order.Status.Should().Be(OrderStatus.Pending);
    }

    [Fact]
    public void Fill_ShouldSetStatusAndTotal()
    {
        var order = new Order(Guid.NewGuid(), "BTCUSDT", OrderSide.Buy, OrderType.Limit, 1m, 50000m, null);
        order.MarkPending();

        order.Fill(51000m);

        order.Status.Should().Be(OrderStatus.Filled);
        order.FilledQuantity.Should().Be(1m);
        order.AverageFillPrice.Should().Be(51000m);
        order.Total.Should().Be(51000m);
        order.FilledAt.Should().NotBeNull();
    }

    [Fact]
    public void Cancel_ShouldWorkOnlyForPendingOrders()
    {
        var order = new Order(Guid.NewGuid(), "BTCUSDT", OrderSide.Buy, OrderType.Limit, 1m, 50000m, null);
        order.MarkPending();

        order.Cancel();

        order.Status.Should().Be(OrderStatus.Cancelled);
        order.CancelledAt.Should().NotBeNull();
    }

    [Fact]
    public void Cancel_ShouldThrowForFilledOrder()
    {
        var order = new Order(Guid.NewGuid(), "BTCUSDT", OrderSide.Buy, OrderType.Market, 1m, 50000m, null);

        var act = () => order.Cancel();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Fail_ShouldSetStatusToFailed()
    {
        var order = new Order(Guid.NewGuid(), "BTCUSDT", OrderSide.Buy, OrderType.Limit, 1m, 50000m, null);
        order.MarkPending();

        order.Fail();

        order.Status.Should().Be(OrderStatus.Failed);
    }

    [Fact]
    public void Constructor_ShouldRejectInvalidInputs()
    {
        var act1 = () => new Order(Guid.Empty, "BTCUSDT", OrderSide.Buy, OrderType.Market, 1m, 50000m, null);
        act1.Should().Throw<ArgumentException>();

        var act2 = () => new Order(Guid.NewGuid(), "", OrderSide.Buy, OrderType.Market, 1m, 50000m, null);
        act2.Should().Throw<ArgumentException>();

        var act3 = () => new Order(Guid.NewGuid(), "BTCUSDT", OrderSide.Buy, OrderType.Market, 0, 50000m, null);
        act3.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Fill_ShouldRejectInvalidPrice()
    {
        var order = new Order(Guid.NewGuid(), "BTCUSDT", OrderSide.Buy, OrderType.Limit, 1m, 50000m, null);
        order.MarkPending();

        var act = () => order.Fill(0);

        act.Should().Throw<ArgumentException>();
    }
}
