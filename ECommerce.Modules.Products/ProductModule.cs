using ECommerce.Modules.Products.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using ECommerce.Contracts.Interfaces;
using ECommerce.Modules.Products.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace ECommerce.Modules.Products;

public static class ProductModule
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

    public static IServiceCollection AddProductModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var serviceProvider = services.BuildServiceProvider();
        var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
        var logger = loggerFactory?.CreateLogger("ProductModule");

        var usePostgres = !string.IsNullOrEmpty(connectionString) && CanConnectToPostgres(connectionString);

        services.AddDbContext<ProductDbContext>(options =>
        {
            if (usePostgres)
            {
                logger?.LogInformation("ProductModule: Using PostgreSQL connection string: {ConnectionString}",
                    connectionString!.Substring(0, Math.Min(50, connectionString.Length)) + "...");

                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly("ECommerce.Modules.Products");
                });
            }
            else
            {
                if (!string.IsNullOrEmpty(connectionString))
                    logger?.LogWarning("ProductModule: Could not connect to PostgreSQL, falling back to in-memory database");
                else
                    logger?.LogInformation("ProductModule: No connection string found, using in-memory database");
                options.UseInMemoryDatabase("ECommerce.Product");
                options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning));
            }
        });

        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IProductCatalogService, ProductCatalogService>();

        return services;
    }
}
