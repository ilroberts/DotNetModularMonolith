using ECommerce.Modules.Orders.Persistence;
using ECommerce.Modules.Orders.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ECommerce.Modules.Orders;

public static class OrderModule
{
    public static IServiceCollection AddOrderModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var serviceProvider = services.BuildServiceProvider();
        var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
        var logger = loggerFactory?.CreateLogger("OrderModule");

        services.AddDbContext<OrderDbContext>(options =>
        {
            if (!string.IsNullOrEmpty(connectionString))
            {
                logger?.LogInformation("OrderModule: Using PostgreSQL connection string: {ConnectionString}",
                    connectionString.Substring(0, Math.Min(50, connectionString.Length)) + "...");

                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly("ECommerce.Modules.Orders");
                });
            }
            else
            {
                logger?.LogInformation("OrderModule: No connection string found, using in-memory database");
                options.UseInMemoryDatabase("ECommerce.Order");
            }
        });

        services.AddScoped<IOrderService, OrderService>();
        return services;
    }
}
