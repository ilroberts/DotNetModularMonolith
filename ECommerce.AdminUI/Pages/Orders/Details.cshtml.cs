using ECommerce.AdminUI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ECommerce.AdminUI.Pages.Orders
{
    public class DetailsModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(IOrderService orderService, ILogger<DetailsModel> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        public OrderDto Order { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            Order = order;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid id, string newStatus)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            order.Status = newStatus;

            var success = await _orderService.UpdateOrderAsync(id, order);
            if (success)
            {
                TempData["SuccessMessage"] = "Order status updated successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update order status.";
            }

            return RedirectToPage("./Details", new { id });
        }
    }
}
