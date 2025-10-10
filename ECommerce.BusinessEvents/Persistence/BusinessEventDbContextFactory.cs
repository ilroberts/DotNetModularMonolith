using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ECommerce.BusinessEvents.Persistence;

/// <summary>
/// Design-time factory for creating BusinessEventDbContext instances during migrations.
/// This is required for EF Core tooling (migrations, database updates, etc.).
/// </summary>
public class BusinessEventDbContextFactory : IDesignTimeDbContextFactory<BusinessEventDbContext>
{
    public BusinessEventDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BusinessEventDbContext>();

        // Try to find the main app's appsettings.json
        var basePath = Directory.GetCurrentDirectory();
        var appSettingsPath = Path.Combine(basePath, "ECommerceApp");

        // If not found, might be running from the project directory
        if (!Directory.Exists(appSettingsPath))
        {
            appSettingsPath = Path.Combine(basePath, "..", "ECommerceApp");
        }

        // Build configuration to read connection string
        var configBuilder = new ConfigurationBuilder();

        if (Directory.Exists(appSettingsPath))
        {
            configBuilder.SetBasePath(appSettingsPath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true);
        }
        else
        {
            configBuilder.SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true);
        }

        var configuration = configBuilder.AddEnvironmentVariables().Build();
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
        {
            // Fallback connection string for design-time if no appsettings.json is found
            connectionString = "Host=localhost;Port=5432;Database=ecommerce;Username=postgres;Password=postgres";
            Console.WriteLine($"Warning: No connection string found in configuration. Using default: {connectionString}");
        }

        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsAssembly("ECommerce.BusinessEvents");
        });

        return new BusinessEventDbContext(optionsBuilder.Options);
    }
}
