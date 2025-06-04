using ECommerce.AdminUI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ECommerce.AdminUI.Pages.Products;

public class EditModel : PageModel
{
    private readonly ProductService _productService;
    private readonly ILogger<EditModel> _logger;

    public EditModel(ProductService productService, ILogger<EditModel> logger)
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
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var success = await _productService.UpdateProductAsync(Product.Id, Product);
        if (success)
        {
            TempData["SuccessMessage"] = "Product updated successfully.";
            return RedirectToPage("./Index");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Error updating product. Please try again.");
            return Page();
        }
    }
}
