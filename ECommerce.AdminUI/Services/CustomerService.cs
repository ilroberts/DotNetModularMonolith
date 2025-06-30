using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace ECommerce.AdminUI.Services;

public class CustomerService : BaseService, ICustomerService
{
    private readonly HttpClient _httpClient;

    public CustomerService(
        IHttpClientFactory httpClientFactory,
        ILogger<CustomerService> logger,
        IHttpContextAccessor httpContextAccessor,
        IAuthService authService)
        : base(httpContextAccessor, authService, logger)
    {
        _httpClient = httpClientFactory.CreateClient("ModularMonolith");
    }

    public async Task<List<CustomerDto>> GetAllCustomersAsync()
    {
        var token = GetTokenFromSession();
        var username = GetUsernameFromSession();

        if (string.IsNullOrEmpty(token))
        {
            Logger.LogWarning("No auth token available for customer list request");
            return new List<CustomerDto>();
        }

        try
        {
            // Define the API call as a function that takes a token
            async Task<HttpResponseMessage> apiCall(string tkn)
            {
                AddAuthorizationHeader(_httpClient, tkn);
                return await _httpClient.GetAsync("customers");
            }

            // Execute with automatic token refresh
            var httpContext = HttpContextAccessor.HttpContext!;
            var (success, response) = await AuthService.ExecuteWithTokenRefreshAsync(
                apiCall, token, username ?? string.Empty, httpContext);

            if (success && response != null)
            {
                Logger.LogInformation("Successfully retrieved customers from {Url}", _httpClient.BaseAddress + "customers");
                return await response.Content.ReadFromJsonAsync<List<CustomerDto>>() ?? new List<CustomerDto>();
            }

            Logger.LogWarning("Failed to retrieve customers due to authentication issues");
            return new List<CustomerDto>();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving customers");
            return new List<CustomerDto>();
        }
    }

    public async Task<CustomerDto?> GetCustomerByIdAsync(Guid id)
    {
        var token = GetTokenFromSession();
        var username = GetUsernameFromSession();

        if (string.IsNullOrEmpty(token))
        {
            Logger.LogWarning("No auth token available for customer detail request");
            return null;
        }

        try
        {
            // Define the API call as a function that takes a token
            async Task<HttpResponseMessage> apiCall(string tkn)
            {
                AddAuthorizationHeader(_httpClient, tkn);
                return await _httpClient.GetAsync($"customers/{id}");
            }

            // Execute with automatic token refresh
            var httpContext = HttpContextAccessor.HttpContext!;
            var (success, response) = await AuthService.ExecuteWithTokenRefreshAsync(
                apiCall, token, username ?? string.Empty, httpContext);

            if (success && response != null)
            {
                return await response.Content.ReadFromJsonAsync<CustomerDto>();
            }

            Logger.LogWarning("Failed to retrieve customer {CustomerId} due to authentication issues", id);
            return null;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving customer {CustomerId}", id);
            return null;
        }
    }

    public async Task<bool> CreateCustomerAsync(CustomerDto customer)
    {
        var token = GetTokenFromSession();
        var username = GetUsernameFromSession();

        if (string.IsNullOrEmpty(token))
        {
            Logger.LogWarning("No auth token available for create customer request");
            return false;
        }

        try
        {
            var content = new StringContent(
                JsonSerializer.Serialize(customer),
                Encoding.UTF8,
                "application/json");

            // Define the API call as a function that takes a token
            async Task<HttpResponseMessage> apiCall(string tkn)
            {
                AddAuthorizationHeader(_httpClient, tkn);
                return await _httpClient.PostAsync("customers", content);
            }

            // Execute with automatic token refresh
            var httpContext = HttpContextAccessor.HttpContext!;
            var (success, response) = await AuthService.ExecuteWithTokenRefreshAsync(
                apiCall, token, username ?? string.Empty, httpContext);

            if (success && response != null)
            {
                Logger.LogInformation("Successfully created customer");
                return true;
            }

            Logger.LogWarning("Failed to create customer due to authentication issues");
            return false;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating customer");
            return false;
        }
    }

    public async Task<bool> UpdateCustomerAsync(Guid id, CustomerDto customer)
    {
        var token = GetTokenFromSession();
        var username = GetUsernameFromSession();

        if (string.IsNullOrEmpty(token))
        {
            Logger.LogWarning("No auth token available for update customer request");
            return false;
        }

        try
        {
            var content = new StringContent(
                JsonSerializer.Serialize(customer),
                Encoding.UTF8,
                "application/json");

            // Define the API call as a function that takes a token
            async Task<HttpResponseMessage> apiCall(string tkn)
            {
                AddAuthorizationHeader(_httpClient, tkn);
                return await _httpClient.PutAsync($"customers/{id}", content);
            }

            // Execute with automatic token refresh
            var httpContext = HttpContextAccessor.HttpContext!;
            var (success, response) = await AuthService.ExecuteWithTokenRefreshAsync(
                apiCall, token, username ?? string.Empty, httpContext);

            if (success && response != null)
            {
                Logger.LogInformation("Successfully updated customer {CustomerId}", id);
                return true;
            }

            Logger.LogWarning("Failed to update customer {CustomerId} due to authentication issues", id);
            return false;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating customer {CustomerId}", id);
            return false;
        }
    }

    /// <summary>
    /// Placeholder method for deleting a customer.
    /// Note: The backend API doesn't support deletion yet.
    /// </summary>
    public async Task<bool> DeleteCustomerAsync(Guid id)
    {
        Logger.LogWarning("DeleteCustomerAsync was called, but the backend API doesn't support deletion yet. Customer ID: {CustomerId}", id);
        return false;
    }
}
