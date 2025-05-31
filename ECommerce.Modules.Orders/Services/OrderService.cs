using ECommerce.Contracts.Interfaces;
using ECommerce.Modules.Orders.Domain;
using ECommerce.Modules.Orders.Persistence;
using ECommerce.Modules.Orders.Result;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Modules.Orders.Services;

internal class OrderService(OrderDbContext orderDbContext,
  ILogger<OrderService> logger,
  IProductCatalogService productCatalogService,
  ICustomerCatalogService customerCatalogService) : IOrderService
{
    private readonly OrderDbContext _orderDbContext = orderDbContext;
    private readonly ILogger<OrderService> _logger = logger;
    private readonly IProductCatalogService _productCatalogService = productCatalogService;
    private readonly ICustomerCatalogService _customerCatalogService = customerCatalogService;

    public async Task<OrderResult> CreateOrderAsync(Guid customerId, List<OrderItem> items)
    {
        try
        {
            var productTasks = items.Select(item => _productCatalogService.GetProductByIdAsync(item.ProductId)).ToList();
            var products = await Task.WhenAll(productTasks);

            if (products.Any(product => product == null))
            {
                throw new KeyNotFoundException("One or more products not found");
            }

            bool customerExists = await _customerCatalogService.CustomerExistsAsync(customerId);
            if (!customerExists)
            {
                throw new KeyNotFoundException($"Customer with id {customerId} not found");
            }
            var itemList = items.Select((item, index) => new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Price = products[index].Price
            }).ToList();

            var order = new Order(customerId, itemList);
            await _orderDbContext.Orders.AddAsync(order);
            await _orderDbContext.SaveChangesAsync();

            return new OrderResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return new OrderResult(false, ex.Message);
        }
    }

    public async Task<Order> GetOrderByIdAsync(Guid orderId)
    {
        return await _orderDbContext.Orders.FindAsync(orderId);
    }

    public async Task<IEnumerable<Order>> GetAllOrdersAsync()
    {
        var orders = await _orderDbContext.Orders.ToListAsync();
        _logger.LogInformation($"Number of orders to be returned: {orders.Count()}");
        return orders;
    }
}
