using ECommerce.AdminUI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECommerce.AdminUI.Pages.Orders
{
    public class CreateModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(
            IOrderService orderService,
            ICustomerService customerService,
            IProductService productService,
            ILogger<CreateModel> logger)
        {
            _orderService = orderService;
            _customerService = customerService;
            _productService = productService;
            _logger = logger;
        }

        [BindProperty]
        public OrderDto Order { get; set; } = new();

        public List<SelectListItem> CustomerOptions { get; set; } = new();
        public List<SelectListItem> ProductOptions { get; set; } = new();

        // This will be serialized to JSON for client-side use
        public List<ProductDataDto> ProductData { get; set; } = new();

        public async Task OnGetAsync()
        {
            await LoadDropdownDataAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownDataAsync();
                return Page();
            }

            // Set additional order properties
            if (Order.CustomerId != Guid.Empty)
            {
                var customer = await _customerService.GetCustomerByIdAsync(Order.CustomerId);
                if (customer != null)
                {
                    Order.CustomerName = customer.Name;
                    Order.CustomerEmail = customer.Email;
                }
            }

            if (Order.ProductId != Guid.Empty)
            {
                var product = await _productService.GetProductByIdAsync(Order.ProductId);
                if (product != null)
                {
                    Order.ProductName = product.Name;
                    Order.ProductPrice = product.Price;
                }
            }

            // Calculate total price as a safeguard (should already be set by client-side)
            Order.TotalPrice = Order.ProductPrice * Order.Quantity;

            // Set created date
            Order.CreatedAt = DateTime.UtcNow;

            var success = await _orderService.CreateOrderAsync(Order);
            if (success)
            {
                TempData["SuccessMessage"] = "Order created successfully.";
                return RedirectToPage("./Index");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Error creating order. Please try again.");
                await LoadDropdownDataAsync();
                return Page();
            }
        }

        private async Task LoadDropdownDataAsync()
        {
            // Load customers for dropdown
            var customers = await _customerService.GetAllCustomersAsync();
            CustomerOptions = customers
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.Name} ({c.Email})"
                })
                .ToList();

            // Load products for dropdown
            var products = await _productService.GetAllProductsAsync();
            ProductOptions = products
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name
                })
                .ToList();

            // Prepare product data for client-side use
            ProductData = products
                .Select(p => new ProductDataDto
                {
                    Id = p.Id.ToString(),
                    Name = p.Name,
                    Price = p.Price,
                    Stock = 100 // Since we don't have real stock data, using placeholder
                })
                .ToList();
        }

        // Helper class for passing product data to client-side JavaScript
        public class ProductDataDto
        {
            public string Id { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public int Stock { get; set; }
        }
    }
}
