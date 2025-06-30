using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace ECommerce.AdminUI.Services;

public class ProductService : BaseService, IProductService
{
    private readonly HttpClient _httpClient;

    public ProductService(
        IHttpClientFactory httpClientFactory,
        ILogger<ProductService> logger,
        IHttpContextAccessor httpContextAccessor,
        IAuthService authService)
        : base(httpContextAccessor, authService, logger)
    {
        _httpClient = httpClientFactory.CreateClient("ModularMonolith");
    }

    public async Task<List<ProductDto>> GetAllProductsAsync()
    {
        var token = GetTokenFromSession();
        var username = GetUsernameFromSession();

        if (string.IsNullOrEmpty(token))
        {
            Logger.LogWarning("No auth token available for product list request");
            return new List<ProductDto>();
        }

        try
        {
            // Define the API call as a function that takes a token
            async Task<HttpResponseMessage> apiCall(string tkn)
            {
                AddAuthorizationHeader(_httpClient, tkn);
                return await _httpClient.GetAsync("products");
            }

            // Execute with automatic token refresh
            var httpContext = HttpContextAccessor.HttpContext!;
            var (success, response) = await AuthService.ExecuteWithTokenRefreshAsync(
                apiCall, token, username ?? string.Empty, httpContext);

            if (success && response != null)
            {
                Logger.LogInformation("Successfully retrieved products from {Url}", _httpClient.BaseAddress + "products");
                return await response.Content.ReadFromJsonAsync<List<ProductDto>>() ?? new List<ProductDto>();
            }

            Logger.LogWarning("Failed to retrieve products due to authentication issues");
            return new List<ProductDto>();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving products");
            return new List<ProductDto>();
        }
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid id)
    {
        var token = GetTokenFromSession();
        var username = GetUsernameFromSession();

        if (string.IsNullOrEmpty(token))
        {
            Logger.LogWarning("No auth token available for product detail request");
            return null;
        }

        try
        {
            // Define the API call as a function that takes a token
            async Task<HttpResponseMessage> apiCall(string tkn)
            {
                AddAuthorizationHeader(_httpClient, tkn);
                return await _httpClient.GetAsync($"products/{id}");
            }

            // Execute with automatic token refresh
            var httpContext = HttpContextAccessor.HttpContext!;
            var (success, response) = await AuthService.ExecuteWithTokenRefreshAsync(
                apiCall, token, username ?? string.Empty, httpContext);

            if (success && response != null)
            {
                return await response.Content.ReadFromJsonAsync<ProductDto>();
            }

            Logger.LogWarning("Failed to retrieve product {ProductId} due to authentication issues", id);
            return null;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving product {ProductId}", id);
            return null;
        }
    }

    public async Task<bool> CreateProductAsync(ProductDto product)
    {
        Logger.LogInformation("creating product with name: {ProductName}", product.Name);

        var token = GetTokenFromSession();
        var username = GetUsernameFromSession();

        if (string.IsNullOrEmpty(token))
        {
            Logger.LogWarning("No auth token available for create product request");
            return false;
        }

        try
        {
            var content = new StringContent(
                JsonSerializer.Serialize(product),
                Encoding.UTF8,
                "application/json");

            // Define the API call as a function that takes a token
            async Task<HttpResponseMessage> apiCall(string tkn)
            {
                AddAuthorizationHeader(_httpClient, tkn);
                return await _httpClient.PostAsync("products", content);
            }

            // Execute with automatic token refresh
            var httpContext = HttpContextAccessor.HttpContext!;
            var (success, response) = await AuthService.ExecuteWithTokenRefreshAsync(
                apiCall, token, username ?? string.Empty, httpContext);

            if (success && response != null)
            {
                Logger.LogInformation("Successfully created product");
                return true;
            }

            Logger.LogWarning("Failed to create product due to authentication issues");
            return false;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating product");
            return false;
        }
    }

    public async Task<bool> UpdateProductAsync(Guid id, ProductDto product)
    {
        var token = GetTokenFromSession();
        var username = GetUsernameFromSession();

        if (string.IsNullOrEmpty(token))
        {
            Logger.LogWarning("No auth token available for update product request");
            return false;
        }

        try
        {
            var content = new StringContent(
                JsonSerializer.Serialize(product),
                Encoding.UTF8,
                "application/json");

            // Define the API call as a function that takes a token
            async Task<HttpResponseMessage> apiCall(string tkn)
            {
                AddAuthorizationHeader(_httpClient, tkn);
                return await _httpClient.PutAsync($"products/{id}", content);
            }

            // Execute with automatic token refresh
            var httpContext = HttpContextAccessor.HttpContext!;
            var (success, response) = await AuthService.ExecuteWithTokenRefreshAsync(
                apiCall, token, username ?? string.Empty, httpContext);

            if (success && response != null)
            {
                Logger.LogInformation("Successfully updated product {ProductId}", id);
                return true;
            }

            Logger.LogWarning("Failed to update product {ProductId} due to authentication issues", id);
            return false;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating product {ProductId}", id);
            return false;
        }
    }

    /// <summary>
    /// Placeholder method for deleting a product.
    /// Note: The backend API doesn't support deletion yet.
    /// </summary>
    public async Task<bool> DeleteProductAsync(Guid id)
    {
        Logger.LogWarning("DeleteProductAsync was called, but the backend API doesn't support deletion yet. Product ID: {ProductId}", id);
        return false;
    }
}
