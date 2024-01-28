using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Converter;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class SimpleAuthAuthorizeAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.HttpContext.Request.Cookies.TryGetValue("UserId", out _))
        {
            context.Result = new UnauthorizedResult();
        }

        base.OnActionExecuting(context);
    }
}