using System.Security.Claims;

namespace Converter;

public static class ClaimsPrincipalExtensions
{
    public static string GetUserId(this ClaimsPrincipal principal)
    {
        var userId = principal.FindFirst("UserId");
        if (userId is null) throw new Exception("User unauthorized");
        return userId.Value;
    }
}