using System.Security.Claims;

namespace InvoiceKeeper.Api.Controllers;

internal static class AuthUserExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier)
            ?? user.FindFirst("sub")
            ?? throw new InvalidOperationException("Идентификатор пользователя не найден в токене.");

        return Guid.Parse(claim.Value);
    }
}

