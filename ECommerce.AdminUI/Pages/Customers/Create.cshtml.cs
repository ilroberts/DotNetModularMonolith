using ECommerce.AdminUI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ECommerce.AdminUI.Pages.Customers;

public class CreateModel : PageModel
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(ICustomerService customerService, ILogger<CreateModel> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    [BindProperty]
    public CustomerDto Customer { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var success = await _customerService.CreateCustomerAsync(Customer);
        if (success)
        {
            TempData["SuccessMessage"] = "Customer created successfully.";
            return RedirectToPage("./Index");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Error creating customer. Please try again.");
            return Page();
        }
    }
}
