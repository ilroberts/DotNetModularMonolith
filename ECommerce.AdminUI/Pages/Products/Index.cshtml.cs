using ECommerce.AdminUI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ECommerce.AdminUI.Pages.Products;

public class IndexModel : PageModel
{
    private readonly IProductService? _productService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(IProductService productService, ILogger<IndexModel> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    public List<ProductDto> Products { get; set; } = new();

    public async Task OnGetAsync()
    {
        Products = await _productService.GetAllProductsAsync();
    }
}
