using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ECommerce.AdminUI.Services;

namespace ECommerce.AdminUI.Pages;

public class IndexModel(DashboardService dashboardService) : PageModel
{
    public DashboardStats Stats { get; private set; } = new DashboardStats();
    public string Username { get; private set; } = string.Empty;

    public async Task OnGetAsync()
    {
        Username = HttpContext.Session.GetString("Username") ?? "Admin";
        Stats = await dashboardService.GetDashboardStatsAsync();
    }

    public async Task<IActionResult> OnGetPatchAsync(Guid eventId)
    {
        var patch = await dashboardService.GetEventPatchAsync(eventId);
        return (patch == null) ? new JsonResult(Array.Empty<object>()) : new JsonResult(patch);
    }
}
