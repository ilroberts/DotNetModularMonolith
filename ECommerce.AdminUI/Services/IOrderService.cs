namespace ECommerce.AdminUI.Services
{
    public interface IOrderService
    {
        Task<List<OrderDto>> GetAllOrdersAsync();
        Task<OrderDto?> GetOrderByIdAsync(Guid id);
        Task<bool> CreateOrderAsync(OrderDto order);
        Task<bool> UpdateOrderAsync(Guid id, OrderDto order);
        Task<bool> DeleteOrderAsync(Guid id);
    }
}

