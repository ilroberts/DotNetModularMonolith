using ECommerceApp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ECommerce.Modules.Customers.Persistence;
using System.Linq;

namespace ECommerce.Pact.ProviderTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        // Replace the real database context with an in-memory database for testing
        builder.ConfigureServices(services =>
        {
            // Find the service descriptor for CustomerDbContext
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<CustomerDbContext>));

            if (dbContextDescriptor != null)
            {
                // Remove the registered service
                services.Remove(dbContextDescriptor);
            }

            // Add a new service with in-memory database
            services.AddDbContext<CustomerDbContext>(options =>
            {
                options.UseInMemoryDatabase("CustomerTestDb");
            });

            // Get access to the service provider to initialize the test database
            var sp = services.BuildServiceProvider();

            // Create a scope to obtain a reference to the database context
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<CustomerDbContext>();

            // Ensure the database is created
            db.Database.EnsureCreated();

            // Initialize with test data if needed
            InitializeTestDatabase(db);
        });
    }

    private void InitializeTestDatabase(CustomerDbContext dbContext)
    {
        // Clear existing data
        dbContext.Customers.RemoveRange(dbContext.Customers);
        dbContext.SaveChanges();

        // Add test data as needed for your Pact tests
        // This data should match what your Pact expectations require

        // Example:
        // For provider states that need specific customers:
        // var testCustomer = new Customer
        // {
        //     Id = Guid.Parse("3f8e05ed-731f-49e6-93a5-d96d45bf9bfd"),
        //     Name = "John Doe",
        //     Email = "john.doe@example.com"
        // };
        // dbContext.Customers.Add(testCustomer);
        // dbContext.SaveChanges();
    }
}
