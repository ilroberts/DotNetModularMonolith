using ECommerce.AdminUI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ECommerce.AdminUI.Pages.Customers;

public class EditModel : PageModel
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<EditModel> _logger;

    public EditModel(ICustomerService customerService, ILogger<EditModel> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    [BindProperty]
    public CustomerDto Customer { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var customer = await _customerService.GetCustomerByIdAsync(id);
        if (customer == null)
        {
            return NotFound();
        }

        Customer = customer;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var success = await _customerService.UpdateCustomerAsync(Customer.Id, Customer);
        if (success)
        {
            TempData["SuccessMessage"] = "Customer updated successfully.";
            return RedirectToPage("./Index");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Error updating customer. Please try again.");
            return Page();
        }
    }
}
