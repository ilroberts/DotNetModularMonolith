
using ECommerce.Modules.Orders.Domain;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Orders.Persistence;

public class OrderDbContext : DbContext
{
  public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
  {
  }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(order =>
       {
            order.HasKey(o => o.Id);
            order.Property(o => o.CustomerId).IsRequired();
            order.OwnsMany(o => o.Items, item =>
            {
                item.HasKey(i => i.Id);
                item.Property(i => i.ProductId).IsRequired();
                item.WithOwner().HasForeignKey("OrderId");
                item.Property(i => i.Quantity);
                item.Property(i => i.Price);
            });
      });

    }

    public DbSet<Order> Orders { get; set; }
}

