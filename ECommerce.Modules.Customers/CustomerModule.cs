using ECommerce.Contracts.Interfaces;
using ECommerce.Modules.Customers.Persistence;
using ECommerce.Modules.Customers.Services;
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

        return services;
    }
}
