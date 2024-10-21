using ECommerce.Modules.Products;
using ECommerce.Modules.Products.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using ECommerce.Common.Interfaces;
using ECommerce.Modules.Products.Domain;
using ECommerce.Common.Infrastructure;
using ECommerce.Contracts.Interfaces;

namespace ECommerce.Modules.Products;

public static class ProductModule
{
  public static IServiceCollection AddProductModule(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddScoped<IProductService, ProductService>();
    services.AddScoped<IProductCatalogService, ProductCatalogService>();
    services.AddSingleton(typeof(IRepository<Product>), typeof(InMemoryRepository<Product>));
    return services;
  }
}
