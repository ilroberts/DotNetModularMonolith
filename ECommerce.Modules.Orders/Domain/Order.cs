using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Orders.Domain
{
    public class Order
  {
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public ICollection<OrderItem> Items { get; set; } = [];
  }

  [Owned]
  public class OrderItem
  {
    public Guid Id { get; set; }

        public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
  }
}
