using ECommerce.Modules.Products.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using ECommerce.Contracts.Interfaces;
using ECommerce.Modules.Products.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Products;

public static class ProductModule
{
    public static IServiceCollection AddProductModule(this IServiceCollection services, IConfiguration configuration)
  {

    services.AddDbContext<ProductDbContext>(options =>
    {
      options.UseInMemoryDatabase("ECommerce.Product");
    });

    services.AddScoped<IProductService, ProductService>();
    services.AddScoped<IProductCatalogService, ProductCatalogService>();

        return services;
  }
}
