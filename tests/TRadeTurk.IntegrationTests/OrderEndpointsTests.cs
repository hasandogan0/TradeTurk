using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;

namespace TRadeTurk.IntegrationTests;

public class OrderEndpointsTests
{
    [Fact]
    public async Task MarketOrder_ShouldExecuteImmediatelyAndCreateHistory()
    {
        await using var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();
        await client.AuthenticateAsSeededUserAsync();

        var response = await client.PostAsJsonAsync("/api/orders", new
        {
            symbol = "BTCUSDT",
            side = "BUY",
            type = "MARKET",
            quantity = 0.1m,
            price = 50000m
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<OrderResultResponse>();
        result!.IsSuccess.Should().BeTrue();
        result.Order!.Status.Should().Be("FILLED");

        var history = await client.GetFromJsonAsync<List<OrderResponse>>("/api/orders/history");
        history.Should().Contain(o => o.Id == result.Order.Id && o.Status == "FILLED");
    }

    [Fact]
    public async Task LimitOrder_ShouldStayPendingAndCanBeCancelled()
    {
        await using var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();
        await client.AuthenticateAsSeededUserAsync();

        var response = await client.PostAsJsonAsync("/api/orders", new
        {
            symbol = "BTCUSDT",
            side = "BUY",
            type = "LIMIT",
            quantity = 0.1m,
            price = 10000m
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<OrderResultResponse>();
        result!.Order!.Status.Should().Be("PENDING");

        var open = await client.GetFromJsonAsync<List<OrderResponse>>("/api/orders/open");
        open.Should().Contain(o => o.Id == result.Order.Id);

        var cancel = await client.DeleteAsync($"/api/orders/{result.Order.Id}");
        cancel.StatusCode.Should().Be(HttpStatusCode.OK);
        var cancelled = await cancel.Content.ReadFromJsonAsync<OrderResultResponse>();
        cancelled!.Order!.Status.Should().Be("CANCELLED");
    }

    [Fact]
    public async Task FilledOrder_ShouldNotBeCancelled()
    {
        await using var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();
        await client.AuthenticateAsSeededUserAsync();

        var create = await client.PostAsJsonAsync("/api/orders", new
        {
            symbol = "BTCUSDT",
            side = "SELL",
            type = "MARKET",
            quantity = 0.1m,
            price = 50000m
        });

        var result = await create.Content.ReadFromJsonAsync<OrderResultResponse>();
        var cancel = await client.DeleteAsync($"/api/orders/{result!.Order!.Id}");
        cancel.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task InsufficientBalance_ShouldNotCreateOrder()
    {
        await using var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();
        await client.AuthenticateAsSeededUserAsync();

        var response = await client.PostAsJsonAsync("/api/orders", new
        {
            symbol = "BTCUSDT",
            side = "BUY",
            type = "MARKET",
            quantity = 99m,
            price = 50000m
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var result = await response.Content.ReadFromJsonAsync<OrderResultResponse>();
        result!.Message.Should().Contain("Yetersiz");
    }

    [Fact]
    public async Task User_ShouldNotSeeAnotherUsersOrder()
    {
        await using var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();
        await client.AuthenticateAsSeededUserAsync();

        var create = await client.PostAsJsonAsync("/api/orders", new
        {
            symbol = "BTCUSDT",
            side = "BUY",
            type = "LIMIT",
            quantity = 0.1m,
            price = 10000m
        });
        var created = await create.Content.ReadFromJsonAsync<OrderResultResponse>();

        var register = await client.PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "Second User",
            email = "second@example.com",
            userName = "seconduser",
            password = "Test12345!"
        });
        var auth = await register.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);

        var get = await client.GetAsync($"/api/orders/{created!.Order!.Id}");
        get.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private sealed class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
    }

    private sealed class OrderResultResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public OrderResponse? Order { get; set; }
    }

    private sealed class OrderResponse
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
