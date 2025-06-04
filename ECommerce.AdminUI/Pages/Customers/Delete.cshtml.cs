using ECommerce.AdminUI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ECommerce.AdminUI.Pages.Customers;

public class DeleteModel : PageModel
{
    private readonly CustomerService _customerService;
    private readonly ILogger<DeleteModel> _logger;

    public DeleteModel(CustomerService customerService, ILogger<DeleteModel> logger)
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
        var id = Customer.Id;
        var success = await _customerService.DeleteCustomerAsync(id);
        
        if (success)
        {
            TempData["SuccessMessage"] = "Customer deleted successfully.";
            return RedirectToPage("./Index");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Error deleting customer. Please try again.");
            return Page();
        }
    }
}
