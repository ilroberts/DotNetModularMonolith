using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace ECommerce.AdminUI.Services;

public class CustomerService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CustomerService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CustomerService(IHttpClientFactory httpClientFactory, ILogger<CustomerService> logger, IHttpContextAccessor httpContextAccessor)
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

    public async Task<List<CustomerDto>> GetAllCustomersAsync()
    {
        try
        {
            AddAuthorizationHeader();
            // Remove the leading slash to use the full base URL from configuration
            var response = await _httpClient.GetAsync("customers");
            _logger.LogInformation("Getting all customers from {Url}", _httpClient.BaseAddress + "customers");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<CustomerDto>>() ?? new List<CustomerDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customers");
            return new List<CustomerDto>();
        }
    }

    public async Task<CustomerDto?> GetCustomerByIdAsync(Guid id)
    {
        try
        {
            AddAuthorizationHeader();
            // Remove the leading slash
            var response = await _httpClient.GetAsync($"customers/{id}");
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<CustomerDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer {CustomerId}", id);
            return null;
        }
    }

    public async Task<bool> CreateCustomerAsync(CustomerDto customer)
    {
        try
        {
            AddAuthorizationHeader();
            var content = new StringContent(
                JsonSerializer.Serialize(customer),
                Encoding.UTF8,
                "application/json");
            
            // Remove the leading slash
            _logger.LogInformation("Creating customer at URL: {Url}", _httpClient.BaseAddress + "customers");
            var response = await _httpClient.PostAsync("customers", content);
            response.EnsureSuccessStatusCode();
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer");
            return false;
        }
    }

    public async Task<bool> UpdateCustomerAsync(Guid id, CustomerDto customer)
    {
        try
        {
            AddAuthorizationHeader();
            var content = new StringContent(
                JsonSerializer.Serialize(customer),
                Encoding.UTF8,
                "application/json");
            
            // Remove the leading slash
            var response = await _httpClient.PutAsync($"customers/{id}", content);
            response.EnsureSuccessStatusCode();
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer {CustomerId}", id);
            return false;
        }
    }

    public async Task<bool> DeleteCustomerAsync(Guid id)
    {
        try
        {
            AddAuthorizationHeader();
            // Remove the leading slash
            var response = await _httpClient.DeleteAsync($"customers/{id}");
            response.EnsureSuccessStatusCode();
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer {CustomerId}", id);
            return false;
        }
    }
}
