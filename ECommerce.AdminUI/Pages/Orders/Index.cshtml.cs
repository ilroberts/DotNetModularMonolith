using ECommerce.AdminUI.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ECommerce.AdminUI.Pages.Orders
{
    public class IndexModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IOrderService orderService, ILogger<IndexModel> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        public List<OrderDto> Orders { get; private set; } = new();

        public async Task OnGetAsync()
        {
            _logger.LogInformation("Retrieving all orders");
            Orders = await _orderService.GetAllOrdersAsync();
        }
    }
}
