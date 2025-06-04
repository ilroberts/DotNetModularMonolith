using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace ECommerce.AdminUI.Services;

public class ProductService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ProductService(IHttpClientFactory httpClientFactory, ILogger<ProductService> logger, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClientFactory.CreateClient("ModularMonolith");
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    private void AddAuthorizationHeader()
    {
        var token = _httpContextAccessor.HttpContext?.Session.GetString("AuthToken");
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        }
    }

    public async Task<List<ProductDto>> GetAllProductsAsync()
    {
        try
        {
            AddAuthorizationHeader();
            _logger.LogInformation("Getting all products from {Url}", _httpClient.BaseAddress + "products");
            var response = await _httpClient.GetAsync("products");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<ProductDto>>() ?? new List<ProductDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products");
            return new List<ProductDto>();
        }
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid id)
    {
        try
        {
            AddAuthorizationHeader();
            var response = await _httpClient.GetAsync($"products/{id}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<ProductDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product {ProductId}", id);
            return null;
        }
    }

    public async Task<bool> CreateProductAsync(ProductDto product)
    {
        _logger.LogInformation("creating product with name: {ProductName}", product.Name);

        try
        {
            AddAuthorizationHeader();
            var content = new StringContent(
                JsonSerializer.Serialize(product),
                Encoding.UTF8,
                "application/json");

            _logger.LogInformation("Creating product at URL: {Url}", _httpClient.BaseAddress + "products");
            var response = await _httpClient.PostAsync("products", content);
            response.EnsureSuccessStatusCode();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return false;
        }
    }

    public async Task<bool> UpdateProductAsync(Guid id, ProductDto product)
    {
        try
        {
            AddAuthorizationHeader();
            var content = new StringContent(
                JsonSerializer.Serialize(product),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PutAsync($"products/{id}", content);
            response.EnsureSuccessStatusCode();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", id);
            return false;
        }
    }

    public async Task<bool> DeleteProductAsync(Guid id)
    {
        try
        {
            AddAuthorizationHeader();
            var response = await _httpClient.DeleteAsync($"products/{id}");
            response.EnsureSuccessStatusCode();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", id);
            return false;
        }
    }
}
