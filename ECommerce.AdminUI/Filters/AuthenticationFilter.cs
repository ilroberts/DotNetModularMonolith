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
        var httpContext = context.HttpContext;

        logger.LogDebug("AuthFilter: Page: {Page}, PathBase: {PathBase}, Path: {Path}",
            context.HandlerInstance?.GetType().Name,
            httpContext.Request.PathBase,
            httpContext.Request.Path);

        // Skip authentication check for Login page to avoid infinite redirect loop
        // Also skip for HealthCheck endpoint used by Kubernetes probes
        if (context.HandlerInstance is Pages.LoginModel or Pages.HealthCheckModel)
        {
            logger.LogDebug("AuthFilter: Skipping auth check for login/health page");
            return;
        }

        // Try to get token from session
        var token = httpContext.Session.GetString("AuthToken");

        // Log session ID and token for debugging
        logger.LogDebug("AuthFilter: Session ID: {SessionID}, Token present: {HasToken}, CookieCount: {CookieCount}",
            httpContext.Session.Id,
            !string.IsNullOrEmpty(token),
            httpContext.Request.Cookies?.Count ?? 0);

        if (!string.IsNullOrEmpty(token))
        {
            logger.LogDebug("AuthFilter: User authenticated with token, allowing access");
            return;
        }

        logger.LogInformation("Unauthenticated access attempt to {Path}", httpContext.Request.PathBase + httpContext.Request.Path);

        // Build the login URL with the correct path base using string concatenation
        string loginPath;
        if (!string.IsNullOrEmpty(httpContext.Request.PathBase))
        {
            // Make sure we don't end up with double slashes
            loginPath = $"{httpContext.Request.PathBase}/Login";
        }
        else
        {
            loginPath = "/Login";
        }

        logger.LogDebug("AuthFilter: Redirecting to {LoginPath}", loginPath);
        context.Result = new RedirectResult(loginPath);
    }

    public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
    {
        // No action needed
    }
}
