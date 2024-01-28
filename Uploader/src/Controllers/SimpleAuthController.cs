using Microsoft.AspNetCore.Mvc;

namespace Converter.Controllers;

[ApiController]
[Route("/api/v1")]
public class SimpleAuthController : ControllerBase
{
    [HttpGet("authenticate")]
    public void Authenticate()
    {
        if (!Request.Cookies.TryGetValue("UserId", out var userId))
        {
            userId = Guid.NewGuid().ToString();
            Response.Cookies.Append("UserId", userId);
        }
    }
}