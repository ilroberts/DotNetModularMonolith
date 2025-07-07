using ECommerce.AdminUI.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ECommerce.AdminUI.Pages.Products;

public class IndexModel : PageModel
{
    private readonly IProductService? _productService;

    public IndexModel(IProductService productService)
    {
        _productService = productService;
    }

    public List<ProductDto> Products { get; set; } = new();

    public async Task OnGetAsync()
    {
        Products = await _productService!.GetAllProductsAsync();
    }
}
