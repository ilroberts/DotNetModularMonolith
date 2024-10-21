
using ECommerce.Modules.Customers.Domain;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Customers.Persistence
{
    public class CustomerDbContext : DbContext
    {
        public CustomerDbContext(DbContextOptions<CustomerDbContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
    }
}
