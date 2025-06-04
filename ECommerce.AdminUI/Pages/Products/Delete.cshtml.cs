using ECommerce.AdminUI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ECommerce.AdminUI.Pages.Products;

public class DeleteModel : PageModel
{
    private readonly ProductService _productService;
    private readonly ILogger<DeleteModel> _logger;

    public DeleteModel(ProductService productService, ILogger<DeleteModel> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    [BindProperty]
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

    public async Task<IActionResult> OnPostAsync()
    {
        var id = Product.Id;
        var success = await _productService.DeleteProductAsync(id);
        
        if (success)
        {
            TempData["SuccessMessage"] = "Product deleted successfully.";
            return RedirectToPage("./Index");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Error deleting product. Please try again.");
            return Page();
        }
    }
}
