using System.Security.Claims;

namespace Converter;

public class SimpleAuthMiddleware
{
    private readonly RequestDelegate _next;

    public SimpleAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Cookies.TryGetValue("UserId", out var userId))
        {
            var identity = new ClaimsIdentity(new[] { new Claim("UserId", userId) });
            context.User.AddIdentity(identity);
        }

        await _next(context);
    }
}