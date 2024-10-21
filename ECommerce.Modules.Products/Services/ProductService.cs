using ECommerce.Common.Interfaces;
using ECommerce.Modules.Products.Domain;
using ECommerce.Modules.Products.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Products.Services;

public class ProductService : IProductService
{
  private readonly ProductDbContext _productDbContext;
  private readonly ILogger<ProductService> _logger;

  public ProductService(ProductDbContext productDbContext, ILogger<ProductService> logger)
  {
    _productDbContext = productDbContext;
    _logger = logger;
  }

  public async Task<Product> GetProductByIdAsync(Guid productId)
  {
    return await _productDbContext.Products.FindAsync(productId);
  }

  public async Task<IEnumerable<Product>> GetAllProductsAsync()
  {
    var products = await _productDbContext.Products.ToListAsync();
    _logger.LogInformation($"Number of products to be returned: {products.Count()}");
    return products;
  }

  public async Task<Product> AddProductAsync(Product product)
  {
    _productDbContext.Products.Add(product);
    await _productDbContext.SaveChangesAsync();
    return product;
  }
}
