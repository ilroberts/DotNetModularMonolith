using ECommerce.Common.Infrastructure;
using ECommerce.Common.Interfaces;
using ECommerce.Modules.Orders.Domain;
using ECommerce.Modules.Orders.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Modules.Orders;

public static class OrderModule
{
    public static IServiceCollection AddOrderModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped(typeof(IRepository<Order>), typeof(InMemoryRepository<Order>));
        return services;
    }
}


