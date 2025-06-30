using System.Text;
using System.Text.Json;

namespace ECommerce.AdminUI.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IHttpClientFactory httpClientFactory, ILogger<AuthService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("TokenService");
        _logger = logger;
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

    public async Task<string?> RefreshTokenAsync(string token)
    {
        try
        {
            _logger.LogDebug("Attempting to refresh token");

            var request = new HttpRequestMessage(HttpMethod.Post, "/modulith/admin/refreshToken");
            request.Headers.Add("Authorization", $"Bearer {token}");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Token refresh failed with status code {StatusCode}", response.StatusCode);
                return null;
            }

            var newToken = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Token refreshed successfully");
            return !string.IsNullOrWhiteSpace(newToken) ? newToken.Trim() : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return null;
        }
    }

    // Helper method to execute authenticated requests with automatic refresh
    public async Task<(bool Success, HttpResponseMessage? Response)> ExecuteWithTokenRefreshAsync(
        Func<string, Task<HttpResponseMessage>> apiCall,
        string token,
        string username,
        HttpContext httpContext)
    {
        try
        {
            // First attempt with current token
            var response = await apiCall(token);

            // If successful, return the response
            if (response.IsSuccessStatusCode)
            {
                return (true, response);
            }

            // If we get a 401 Unauthorized, try refreshing the token
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger.LogInformation("Received 401 Unauthorized, attempting to refresh token");

                // Try to refresh the token
                var newToken = await RefreshTokenAsync(token);

                if (!string.IsNullOrEmpty(newToken))
                {
                    // Update token in session
                    httpContext.Session.SetString("AuthToken", newToken);

                    // Retry the API call with the new token
                    var retryResponse = await apiCall(newToken);
                    return (retryResponse.IsSuccessStatusCode, retryResponse);
                }

                // If refresh fails, clear the token and return false
                _logger.LogWarning("Token refresh failed, user needs to re-authenticate");
                httpContext.Session.Remove("AuthToken");
                return (false, null);
            }

            // For other error responses, just return as is
            return (false, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing authenticated request");
            return (false, null);
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
