using ECommerce.AdminUI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ECommerce.AdminUI.Pages.Products;

public class DetailsModel : PageModel
{
    private readonly IProductService _productService;
    private readonly ILogger<DetailsModel> _logger;

    public DetailsModel(IProductService productService, ILogger<DetailsModel> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    public ProductDto Product { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        Product = product;
        return Page();
    }
}
