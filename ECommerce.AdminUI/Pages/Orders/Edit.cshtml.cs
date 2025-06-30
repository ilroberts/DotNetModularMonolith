using ECommerce.AdminUI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECommerce.AdminUI.Pages.Orders
{
    public class EditModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;

        public EditModel(
            IOrderService orderService,
            IProductService productService,
            ILogger<EditModel> logger)
        {
            _orderService = orderService;
            _productService = productService;
        }

        [BindProperty]
        public OrderDto Order { get; set; } = new();

        public List<SelectListItem> ProductOptions { get; set; } = new();

        // This will be serialized to JSON for client-side use
        public List<CreateModel.ProductDataDto> ProductData { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            Order = order;
            await LoadProductDataAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadProductDataAsync();
                return Page();
            }

            // Get updated product information if product changed
            if (Order.ProductId != Guid.Empty)
            {
                var product = await _productService.GetProductByIdAsync(Order.ProductId);
                if (product != null)
                {
                    Order.ProductName = product.Name;
                    Order.ProductPrice = product.Price;

                    // Recalculate total price
                    Order.TotalPrice = Order.ProductPrice * Order.Quantity;
                }
            }

            var success = await _orderService.UpdateOrderAsync(Order.Id, Order);
            if (success)
            {
                TempData["SuccessMessage"] = "Order updated successfully.";
                return RedirectToPage("./Index");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Error updating order. Please try again.");
                await LoadProductDataAsync();
                return Page();
            }
        }

        private async Task LoadProductDataAsync()
        {
            // Load products for dropdown
            var products = await _productService.GetAllProductsAsync();
            ProductOptions = products
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name,
                    Selected = p.Id == Order.ProductId
                })
                .ToList();

            // Prepare product data for client-side use
            ProductData = products
                .Select(p => new CreateModel.ProductDataDto
                {
                    Id = p.Id.ToString(),
                    Name = p.Name,
                    Price = p.Price,
                    Stock = 100 // Since we don't have real stock data, using placeholder
                })
                .ToList();
        }
    }
}
