
using ECommerce.Modules.Products.Domain;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Products.Persistence;

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
}

