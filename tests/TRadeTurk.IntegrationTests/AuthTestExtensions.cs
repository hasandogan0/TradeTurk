using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace TRadeTurk.IntegrationTests;

internal static class AuthTestExtensions
{
    public static async Task AuthenticateAsSeededUserAsync(this HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            emailOrUserName = "test@example.com",
            password = "Test12345!"
        });
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result!.Token);
    }

    public sealed class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
    }
}
