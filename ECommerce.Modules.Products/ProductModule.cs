using ECommerce.Modules.Products.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using ECommerce.Contracts.Interfaces;
using ECommerce.Modules.Products.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Modules.Products;

public static class ProductModule
{
    public static IServiceCollection AddProductModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var serviceProvider = services.BuildServiceProvider();
        var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
        var logger = loggerFactory?.CreateLogger("ProductModule");

        services.AddDbContext<ProductDbContext>(options =>
        {
            if (!string.IsNullOrEmpty(connectionString))
            {
                logger?.LogInformation("ProductModule: Using PostgreSQL connection string: {ConnectionString}",
                    connectionString.Substring(0, Math.Min(50, connectionString.Length)) + "...");

                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly("ECommerce.Modules.Products");
                });
            }
            else
            {
                logger?.LogInformation("ProductModule: No connection string found, using in-memory database");
                options.UseInMemoryDatabase("ECommerce.Product");
            }
        });

        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IProductCatalogService, ProductCatalogService>();

        return services;
    }
}
