using Microsoft.AspNetCore.Mvc.Filters;
using SmartCourt.Common;
using SmartCourt.Extensions;

namespace SmartCourt.Common.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class AuthorizeOwnerAttribute : ActionFilterAttribute
{
    private readonly string _routeParameterName;

    public AuthorizeOwnerAttribute(string routeParameterName = "id")
    {
        _routeParameterName = routeParameterName;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.RouteData.Values.TryGetValue(_routeParameterName, out var routeId))
        {
            var idString = routeId?.ToString();
            var currentUserId = context.HttpContext.User.GetUserId();
            var isAdmin = context.HttpContext.User.IsInRole("Admin");

            if (!string.IsNullOrEmpty(idString) && idString != currentUserId && !isAdmin)
            {
                throw new ForbiddenAccessException("غير مصرح لك بإجراء هذه العملية على هذا الملف الشخصي");
            }
        }

        base.OnActionExecuting(context);
    }
}
