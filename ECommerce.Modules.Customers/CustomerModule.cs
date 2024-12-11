using ECommerce.Contracts.Interfaces;
using ECommerce.Modules.Customers.Domain;
using ECommerce.Modules.Customers.Persistence;
using ECommerce.Modules.Customers.Services;
using ECommerce.Modules.Customers.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Modules.Customers;

public static class CustomerModule
{
    public static IServiceCollection AddCustomerModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CustomerDbContext>(options =>
        {
          options.UseInMemoryDatabase("ECommerce.Customer");
        });

        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ICustomerCatalogService, CustomerCatalogService>();
        services.AddSingleton<SuspensionTypeService>();

        services.AddHostedService<DatabaseSeedingHostedService>();

        return services;
    }

    public static void SeedData(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();
        context.Database.EnsureCreated();
        SuspensionTypeSeedData.Seed(context);

        var suspensionTypeService = scope.ServiceProvider.GetRequiredService<SuspensionTypeService>();
        suspensionTypeService.LoadSuspensionTypesAsync().GetAwaiter().GetResult();
    }
}
