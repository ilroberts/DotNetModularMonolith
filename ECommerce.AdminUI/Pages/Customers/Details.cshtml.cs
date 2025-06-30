using ECommerce.AdminUI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ECommerce.AdminUI.Pages.Customers;

public class DetailsModel : PageModel
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<DetailsModel> _logger;

    public DetailsModel(ICustomerService customerService, ILogger<DetailsModel> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

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
}
