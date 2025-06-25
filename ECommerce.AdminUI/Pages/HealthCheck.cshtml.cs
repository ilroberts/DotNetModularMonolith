using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ECommerce.AdminUI.Pages
{
    [AllowAnonymous]
    public class HealthCheckModel : PageModel
    {
        private readonly ILogger<HealthCheckModel> _logger;

        public HealthCheckModel(ILogger<HealthCheckModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            return new JsonResult(new { status = "healthy" });
        }
    }
}
