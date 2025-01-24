using ECommerce.Contracts.Interfaces;
using ECommerce.Modules.Products.Domain;
using ECommerce.Modules.Products.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Products.Services;

public class ProductService : IProductService
{
  private readonly ProductDbContext _productDbContext;
  private readonly ILogger<ProductService> _logger;
  private readonly IOrderCatalogService _orderCatalogService;

  public ProductService(ProductDbContext productDbContext,
      ILogger<ProductService> logger,
      IOrderCatalogService orderCatalogService)
  {
    _productDbContext = productDbContext;
    _logger = logger;
    _orderCatalogService = orderCatalogService;
  }

  public async Task<Product> GetProductByIdAsync(Guid productId)
  {
    return await _productDbContext.Products.FindAsync(productId);
  }

  public async Task<IEnumerable<Product>> GetAllProductsAsync()
  {
    var products = await _productDbContext.Products.ToListAsync();
    _logger.LogInformation($"Number of products to be returned: {products.Count()}");

    // dummy call to OrderCatalogService
    var order = await _orderCatalogService.GetOrderByProductIdAsync(products.First().Id);
    _logger.LogInformation("Order Id returned from product service: {OrderId}", order?.Id);

    return products;
  }

  public async Task<Product> AddProductAsync(Product product)
  {
    _productDbContext.Products.Add(product);
    await _productDbContext.SaveChangesAsync();
    return product;
  }
}
