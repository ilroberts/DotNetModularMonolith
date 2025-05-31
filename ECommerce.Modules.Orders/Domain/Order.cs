using ECommerce.Common.Domain;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Orders.Domain
{
    public class Order : Entity
    {
        public Guid CustomerId { get; set; }
        public ICollection<OrderItem> Items { get; set; }

        // Parameterless constructor for EF Core
        private Order() { }

        public Order(Guid customerId, ICollection<OrderItem> items)
        {
            Id = Guid.NewGuid();
            CustomerId = customerId;
            Items = items ?? throw new ArgumentNullException(nameof(items), "Items cannot be null");
        }
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
