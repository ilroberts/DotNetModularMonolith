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
        _logger.LogInformation("Creating a new product with name: {ProductName}", Product.Name);
        if (!ModelState.IsValid)
        {
            // Log all errors for debugging
            foreach (var key in ModelState.Keys)
            {
                var errors = ModelState[key]?.Errors;
                if (errors != null && errors.Count > 0)
                {
                    foreach (var error in errors)
                    {
                        _logger.LogWarning("ModelState Error for '{Key}': {ErrorMessage}", key, error.ErrorMessage);
                    }
                }
            }
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
