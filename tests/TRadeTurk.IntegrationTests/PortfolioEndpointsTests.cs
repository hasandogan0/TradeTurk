using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace TRadeTurk.IntegrationTests;

public class PortfolioEndpointsTests
{
    [Fact]
    public async Task Register_ShouldCreateWalletVirtualCardAndReturnToken()
    {
        await using var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "Ada Lovelace",
            email = "ada@example.com",
            userName = "ada",
            password = "Test12345!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        auth!.Token.Should().NotBeNullOrWhiteSpace();

        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth.Token);
        var wallet = await client.GetFromJsonAsync<WalletDetailsResponse>("/api/wallet/me");
        wallet!.AvailableBalance.Should().Be(50000m);
        wallet.VirtualCard.Should().NotBeNull();
        wallet.VirtualCard!.MaskedCardNumber.Should().StartWith("**** **** **** ");
    }

    [Fact]
    public async Task WalletAndAssetsEndpoints_ShouldReturnOnlyAuthenticatedUserData()
    {
        await using var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();
        await client.AuthenticateAsSeededUserAsync();

        var walletResponse = await client.GetAsync("/api/wallet/me");
        walletResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var wallet = await walletResponse.Content.ReadFromJsonAsync<WalletDetailsResponse>();
        wallet!.AvailableBalance.Should().Be(100000m);

        var assets = await client.GetFromJsonAsync<List<AssetResponse>>("/api/assets/me");
        assets.Should().ContainSingle(a => a.Symbol == "BTCUSDT" && a.Amount == 2m);
    }

    [Fact]
    public async Task PortfolioSummaryEndpoint_ShouldReturnSummary()
    {
        await using var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();
        await client.AuthenticateAsSeededUserAsync();

        var response = await client.GetAsync("/api/portfolio/summary/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var summary = await response.Content.ReadFromJsonAsync<PortfolioSummaryResponse>();
        summary!.TotalPortfolioValue.Should().BeGreaterThan(100000m);
        summary.AvailableUsdt.Should().Be(100000m);
    }

    [Fact]
    public async Task PortfolioHistoryEndpoint_ShouldReturnAuthenticatedUserHistory()
    {
        await using var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();
        await client.AuthenticateAsSeededUserAsync();

        var response = await client.GetAsync("/api/portfolio/history/me?range=7D");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var history = await response.Content.ReadFromJsonAsync<List<PortfolioHistoryResponse>>();
        history.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateSettingsEndpoint_ShouldUpdateCurrentUser()
    {
        await using var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();
        await client.AuthenticateAsSeededUserAsync();

        var response = await client.PutAsJsonAsync("/api/users/me", new
        {
            fullName = "Updated User",
            email = "updated@example.com",
            userName = "updateduser",
            preferredCurrency = "USDT",
            themePreference = "dark"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<UserResponse>();
        user!.FullName.Should().Be("Updated User");
        user.Email.Should().Be("updated@example.com");
    }

    private sealed class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
    }

    private sealed class WalletDetailsResponse
    {
        public decimal AvailableBalance { get; set; }
        public CardResponse? VirtualCard { get; set; }
    }

    private sealed class CardResponse
    {
        public string MaskedCardNumber { get; set; } = string.Empty;
    }

    private sealed class AssetResponse
    {
        public string Symbol { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    private sealed class PortfolioSummaryResponse
    {
        public decimal TotalPortfolioValue { get; set; }
        public decimal AvailableUsdt { get; set; }
    }

    private sealed class PortfolioHistoryResponse
    {
        public decimal TotalValue { get; set; }
    }

    private sealed class UserResponse
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
