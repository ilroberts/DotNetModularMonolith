using ECommerce.Contracts.Interfaces;
using ECommerce.Modules.Customers.Domain;
using ECommerce.Modules.Customers.Persistence;
using ECommerce.Modules.Customers.Services;
using ECommerce.Modules.Customers.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ECommerce.Modules.Customers;

public static class CustomerModule
{
    public static IServiceCollection AddCustomerModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var serviceProvider = services.BuildServiceProvider();
        var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
        var logger = loggerFactory?.CreateLogger("CustomerModule");

        services.AddDbContext<CustomerDbContext>(options =>
        {
            if (!string.IsNullOrEmpty(connectionString))
            {
                logger?.LogInformation("CustomerModule: Using PostgreSQL connection string: {ConnectionString}",
                    connectionString.Substring(0, Math.Min(50, connectionString.Length)) + "...");

                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly("ECommerce.Modules.Customers");
                });
            }
            else
            {
                logger?.LogInformation("CustomerModule: No connection string found, using in-memory database");
                options.UseInMemoryDatabase("ECommerce.Customer");
            }
        });

        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ICustomerCatalogService, CustomerCatalogService>();

        return services;
    }
}
