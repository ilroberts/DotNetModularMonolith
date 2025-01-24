using ECommerce.Contracts.Interfaces;
using ECommerce.Modules.Orders.Persistence;
using ECommerce.Modules.Orders.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Modules.Orders;

public static class OrderModule
{
    public static IServiceCollection AddOrderModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<OrderDbContext>(options =>
        {
            options.UseInMemoryDatabase("ECommerce.Product");
        });

        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IOrderCatalogService, OrderCatalogService>();
        return services;
    }
}
