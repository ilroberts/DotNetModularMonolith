using ECommerce.AdminUI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ECommerce.AdminUI.Pages.Products;

public class CreateModel : PageModel
{
    private readonly ProductService _productService;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(ProductService productService, ILogger<CreateModel> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    [BindProperty]
    public ProductDto Product { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var success = await _productService.CreateProductAsync(Product);
        if (success)
        {
            TempData["SuccessMessage"] = "Product created successfully.";
            return RedirectToPage("./Index");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Error creating product. Please try again.");
            return Page();
        }
    }
}
