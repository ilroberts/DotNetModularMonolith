
using ECommerce.Modules.Customers.Domain;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Customers.Persistence
{
    public class CustomerDbContext(DbContextOptions<CustomerDbContext> options) : DbContext(options)
    {
        public DbSet<Customer> Customers { get; set; }
    }
}
