using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ECommerce.AdminUI.Services;

namespace ECommerce.AdminUI.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly DashboardService _dashboardService;

    public IndexModel(ILogger<IndexModel> logger, DashboardService dashboardService)
    {
        _logger = logger;
        _dashboardService = dashboardService;
    }

    public DashboardStats Stats { get; private set; } = new DashboardStats();
    public string Username { get; private set; } = string.Empty;

    public async Task OnGetAsync()
    {
        Username = HttpContext.Session.GetString("Username") ?? "Admin";
        Stats = await _dashboardService.GetDashboardStatsAsync();
    }

    public async Task<IActionResult> OnGetPatchAsync(Guid eventId)
    {
        // TODO: Replace with your actual patch-fetching logic
        // Example: var patch = await _dashboardService.GetEventPatchAsync(eventId);
        var patch = await _dashboardService.GetEventPatchAsync(eventId);
        if (patch == null)
        {
            return new JsonResult(Array.Empty<object>());
        }
        return new JsonResult(patch);
    }
}
