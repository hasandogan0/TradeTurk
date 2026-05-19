using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace TRadeTurk.WebAPI.Authentication;

public class JwtAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IConfiguration _configuration;

    public JwtAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IConfiguration configuration)
        : base(options, logger, encoder)
    {
        _configuration = configuration;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authorization = Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authorization) || !authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var token = authorization["Bearer ".Length..].Trim();
        var principal = ValidateToken(token);
        return Task.FromResult(principal == null
            ? AuthenticateResult.Fail("Invalid token.")
            : AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name)));
    }

    private ClaimsPrincipal? ValidateToken(string token)
    {
        var parts = token.Split('.');
        if (parts.Length != 3) return null;

        var secret = _configuration["Jwt:Secret"] ?? "TRadeTurk-development-secret-key-change-me-please";
        var unsignedToken = $"{parts[0]}.{parts[1]}";
        var expectedSignature = Sign(unsignedToken, secret);
        if (!CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(expectedSignature), Encoding.ASCII.GetBytes(parts[2])))
        {
            return null;
        }

        using var payload = JsonDocument.Parse(Base64UrlDecode(parts[1]));
        var root = payload.RootElement;
        var issuer = _configuration["Jwt:Issuer"] ?? "TRadeTurk";
        var audience = _configuration["Jwt:Audience"] ?? "TRadeTurkFrontend";

        if (!HasString(root, "iss", issuer) || !HasString(root, "aud", audience)) return null;
        if (!root.TryGetProperty("exp", out var exp) || exp.GetInt64() < DateTimeOffset.UtcNow.ToUnixTimeSeconds()) return null;
        if (!root.TryGetProperty("sub", out var sub) || !Guid.TryParse(sub.GetString(), out var userId)) return null;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new("sub", userId.ToString())
        };

        AddClaim(root, ClaimTypes.Name, claims);
        AddClaim(root, ClaimTypes.Email, claims);

        return new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name));
    }

    private static bool HasString(JsonElement root, string property, string expected)
    {
        return root.TryGetProperty(property, out var value) && value.GetString() == expected;
    }

    private static void AddClaim(JsonElement root, string claimType, ICollection<Claim> claims)
    {
        if (root.TryGetProperty(claimType, out var value) && !string.IsNullOrWhiteSpace(value.GetString()))
        {
            claims.Add(new Claim(claimType, value.GetString()!));
        }
    }

    private static string Sign(string value, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        return Base64UrlEncode(hmac.ComputeHash(Encoding.UTF8.GetBytes(value)));
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static byte[] Base64UrlDecode(string value)
    {
        var padded = value.Replace('-', '+').Replace('_', '/');
        padded = padded.PadRight(padded.Length + (4 - padded.Length % 4) % 4, '=');
        return Convert.FromBase64String(padded);
    }
}
