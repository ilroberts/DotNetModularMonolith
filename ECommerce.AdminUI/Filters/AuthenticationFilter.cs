using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ECommerce.AdminUI.Filters;

public class AuthenticationFilter(ILogger<AuthenticationFilter> logger) : IPageFilter
{
    public void OnPageHandlerSelected(PageHandlerSelectedContext context)
    {
        // No action needed
    }

    public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        // Skip authentication check for Login page to avoid infinite redirect loop
        // Also skip for HealthCheck endpoint used by Kubernetes probes
        if (context.HandlerInstance is Pages.LoginModel or Pages.HealthCheckModel)
        {
            return;
        }

        var httpContext = context.HttpContext;
        var token = httpContext.Session.GetString("AuthToken");

        if (!string.IsNullOrEmpty(token))
        {
            return;
        }

        logger.LogInformation("Unauthenticated access attempt to {Path}", httpContext.Request.PathBase + httpContext.Request.Path);

        // Build the login URL with the correct path base
        var loginPath = httpContext.Request.PathBase.Add("/Login").ToString();
        context.Result = new RedirectResult(loginPath);
    }

    public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
    {
        // No action needed
    }
}
