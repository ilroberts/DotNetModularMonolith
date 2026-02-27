using ECommerce.Modules.Orders.Persistence;
using ECommerce.Modules.Orders.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace ECommerce.Modules.Orders;

public static class OrderModule
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

    public static IServiceCollection AddOrderModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var serviceProvider = services.BuildServiceProvider();
        var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
        var logger = loggerFactory?.CreateLogger("OrderModule");

        var usePostgres = !string.IsNullOrEmpty(connectionString) && CanConnectToPostgres(connectionString);

        services.AddDbContext<OrderDbContext>(options =>
        {
            if (usePostgres)
            {
                logger?.LogInformation("OrderModule: Using PostgreSQL connection string: {ConnectionString}",
                    connectionString!.Substring(0, Math.Min(50, connectionString.Length)) + "...");

                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly("ECommerce.Modules.Orders");
                });
            }
            else
            {
                if (!string.IsNullOrEmpty(connectionString))
                    logger?.LogWarning("OrderModule: Could not connect to PostgreSQL, falling back to in-memory database");
                else
                    logger?.LogInformation("OrderModule: No connection string found, using in-memory database");
                options.UseInMemoryDatabase("ECommerce.Order");
                options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning));
            }
        });

        services.AddScoped<IOrderService, OrderService>();
        return services;
    }
}
