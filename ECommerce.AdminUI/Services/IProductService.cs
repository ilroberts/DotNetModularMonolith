namespace ECommerce.AdminUI.Services
{
    public interface IProductService
    {
        Task<List<ProductDto>> GetAllProductsAsync();
        Task<ProductDto?> GetProductByIdAsync(Guid id);
        Task<bool> CreateProductAsync(ProductDto product);
        Task<bool> UpdateProductAsync(Guid id, ProductDto product);
        Task<bool> DeleteProductAsync(Guid id);
    }
}

