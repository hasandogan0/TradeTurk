using System.Security.Claims;
using TRadeTurk.Application.Common.Interfaces;

namespace TRadeTurk.WebAPI.Controllers;

internal static class ControllerUserExtensions
{
    public static Guid RequireUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        if (!Guid.TryParse(value, out var userId))
        {
            throw new InvalidOperationException("Oturum bilgisi gecersiz.");
        }

        return userId;
    }

    public static Guid SetCurrentUserFromClaims(this ClaimsPrincipal user, ICurrentUserContext currentUserContext)
    {
        var userId = user.RequireUserId();
        currentUserContext.SetUserId(userId);
        return userId;
    }
}
