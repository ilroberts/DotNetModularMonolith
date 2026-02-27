using ECommerce.Contracts.Interfaces;
using ECommerce.Modules.Customers.Persistence;
using ECommerce.Modules.Customers.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace ECommerce.Modules.Customers;

public static class CustomerModule
{
    private static bool CanConnectToPostgres(string connectionString)
    {
        try
        {
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static IServiceCollection AddCustomerModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var serviceProvider = services.BuildServiceProvider();
        var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
        var logger = loggerFactory?.CreateLogger("CustomerModule");

        var usePostgres = !string.IsNullOrEmpty(connectionString) && CanConnectToPostgres(connectionString);

        services.AddDbContext<CustomerDbContext>(options =>
        {
            if (usePostgres)
            {
                logger?.LogInformation("CustomerModule: Using PostgreSQL connection string: {ConnectionString}",
                    connectionString!.Substring(0, Math.Min(50, connectionString.Length)) + "...");

                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly("ECommerce.Modules.Customers");
                });
            }
            else
            {
                if (!string.IsNullOrEmpty(connectionString))
                    logger?.LogWarning("CustomerModule: Could not connect to PostgreSQL, falling back to in-memory database");
                else
                    logger?.LogInformation("CustomerModule: No connection string found, using in-memory database");
                options.UseInMemoryDatabase("ECommerce.Customer");
                options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning));
            }
        });

        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ICustomerCatalogService, CustomerCatalogService>();

        return services;
    }
}
