using ECommerce.Common;
using ECommerce.Modules.Products.Domain;

namespace ECommerce.Modules.Products.Services;

public interface IProductService
{
  Task<Product> GetProductByIdAsync(Guid productId);
  Task<IEnumerable<Product>> GetAllProductsAsync();
  Task<Result<Product, string>> AddProductAsync(Product product, string userId, string correlationId);
  Task<Result<Product, string>> UpdateProductAsync(Guid id, Product updatedProduct, string userId, string correlationId);
}
