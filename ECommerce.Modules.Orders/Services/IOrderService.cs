using ECommerce.Contracts.DTOs;
using ECommerce.Modules.Orders.Domain;
using ECommerce.Modules.Orders.Result;

namespace ECommerce.Modules.Orders.Services;

public interface IOrderService
{
    Task<OrderResult> CreateOrderAsync(Guid customerId, List<OrderItem> items);
  Task<Order> GetOrderByIdAsync(Guid orderId);
  Task<IEnumerable<Order>> GetAllOrdersAsync();
    OrderDto? GetOrderByProductIdAsync(Guid productId);
}
