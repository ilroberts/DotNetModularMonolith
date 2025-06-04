using ECommerce.AdminUI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ECommerce.AdminUI.Pages.Customers;

public class IndexModel : PageModel
{
    private readonly CustomerService _customerService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(CustomerService customerService, ILogger<IndexModel> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    public List<CustomerDto> Customers { get; set; } = new();

    public async Task OnGetAsync()
    {
        Customers = await _customerService.GetAllCustomersAsync();
    }
}
