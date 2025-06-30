using ECommerce.AdminUI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ECommerce.AdminUI.Pages.Orders
{
    public class DeleteModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<DeleteModel> _logger;

        public DeleteModel(IOrderService orderService, ILogger<DeleteModel> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        [BindProperty]
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

        public async Task<IActionResult> OnPostAsync()
        {
            var success = await _orderService.DeleteOrderAsync(Order.Id);

            if (success)
            {
                TempData["SuccessMessage"] = "Order deleted successfully.";
                return RedirectToPage("./Index");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Error deleting order. Please try again.");
                return Page();
            }
        }
    }
}
