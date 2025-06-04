using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ECommerce.AdminUI.Filters;

public class AuthenticationFilter : IPageFilter
{
    private readonly ILogger<AuthenticationFilter> _logger;
    
    public AuthenticationFilter(ILogger<AuthenticationFilter> logger)
    {
        _logger = logger;
    }
    
    public void OnPageHandlerSelected(PageHandlerSelectedContext context)
    {
        // No action needed
    }

    public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        // Skip authentication check for Login page to avoid infinite redirect loop
        if (context.HandlerInstance is Pages.LoginModel)
        {
            return;
        }
        
        var httpContext = context.HttpContext;
        var token = httpContext.Session.GetString("AuthToken");
        
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogInformation("Unauthenticated access attempt to {Path}", httpContext.Request.Path);
            
            // Redirect to login page
            context.Result = new RedirectToPageResult("/Login");
        }
    }

    public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
    {
        // No action needed
    }
}
