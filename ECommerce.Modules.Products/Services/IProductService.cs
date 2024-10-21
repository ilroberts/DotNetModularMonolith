using ECommerce.Modules.Products.Domain;

namespace ECommerce.Modules.Products.Services;

public interface IProductService
{
  Task<Product> GetProductByIdAsync(Guid productId);
  Task<IEnumerable<Product>> GetAllProductsAsync();
  Task<Product> AddProductAsync(Product product);
}
