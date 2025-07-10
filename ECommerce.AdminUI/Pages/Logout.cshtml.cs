using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ECommerce.AdminUI.Pages;

public class LogoutModel : PageModel
{
    private readonly ILogger<LogoutModel> _logger;

    public LogoutModel(ILogger<LogoutModel> logger)
    {
        _logger = logger;
    }

    public IActionResult OnGet()
    {
        // Log the logout event
        var username = HttpContext.Session.GetString("Username");
        _logger.LogInformation("User {Username} logged out", username);

        // Clear all session data
        HttpContext.Session.Clear();

        // Redirect to login page, respecting PathBase
        var pathBase = HttpContext.Request.PathBase.HasValue ? HttpContext.Request.PathBase.Value : string.Empty;
        return Redirect(pathBase + "/Login");
    }
}
