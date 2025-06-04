using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace ECommerce.AdminUI.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthService> _logger;
    private readonly IConfiguration _configuration;

    public AuthService(IHttpClientFactory httpClientFactory, ILogger<AuthService> logger, IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient("TokenService");
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<string?> GenerateTokenAsync(string userName)
    {
        try
        {
            var request = new { user_name = userName };
            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            _logger.LogInformation("Generating token for user {UserName}, request body: {RequestBody}", 
                userName, JsonSerializer.Serialize(request));
                
            var response = await _httpClient.PostAsync("/modulith/admin/generateToken", content);
            
            // Read the raw response for debugging
            var tokenString = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Response status: {Status}, Content: {Content}", 
                response.StatusCode, tokenString);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Token generation failed with status code {StatusCode}", response.StatusCode);
                return null;
            }
            
            // The response is plain text containing the token, not JSON
            return !string.IsNullOrWhiteSpace(tokenString) ? tokenString.Trim() : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating token for user {UserName}", userName);
            return null;
        }
    }

    public class TokenRequest
    {
        public string UserName { get; set; } = string.Empty;
    }

    public class TokenResponse
    {
        public string? Token { get; set; }
    }
}
