using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace TRadeTurk.IntegrationTests;

public class SecurityEndpointsTests
{
    [Fact]
    public async Task RefreshToken_ShouldReturnNewAccessToken()
    {
        await using var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        var login = await client.PostAsJsonAsync("/api/auth/login", new
        {
            emailOrUserName = "test@example.com",
            password = "Test12345!"
        });
        var auth = await login.Content.ReadFromJsonAsync<AuthResponse>();

        var refresh = await client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken = auth!.RefreshToken });

        refresh.StatusCode.Should().Be(HttpStatusCode.OK);
        var refreshed = await refresh.Content.ReadFromJsonAsync<AuthResponse>();
        refreshed!.Token.Should().NotBeNullOrWhiteSpace();
        refreshed.RefreshToken.Should().NotBe(auth.RefreshToken);
    }

    [Fact]
    public async Task Logout_ShouldRevokeRefreshToken()
    {
        await using var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        var login = await client.PostAsJsonAsync("/api/auth/login", new
        {
            emailOrUserName = "test@example.com",
            password = "Test12345!"
        });
        var auth = await login.Content.ReadFromJsonAsync<AuthResponse>();

        await client.AuthenticateAsSeededUserAsync();
        var logout = await client.PostAsJsonAsync("/api/auth/logout", new { refreshToken = auth!.RefreshToken });
        logout.StatusCode.Should().Be(HttpStatusCode.OK);

        var refresh = await client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken = auth.RefreshToken });
        refresh.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private sealed class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
