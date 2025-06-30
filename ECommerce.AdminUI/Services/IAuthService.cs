using Microsoft.AspNetCore.Http;

namespace ECommerce.AdminUI.Services;

public interface IAuthService
{
    Task<string?> GenerateTokenAsync(string userName);
    Task<string?> RefreshTokenAsync(string token);
    Task<(bool Success, HttpResponseMessage? Response)> ExecuteWithTokenRefreshAsync(
        Func<string, Task<HttpResponseMessage>> apiCall,
        string token,
        string username,
        HttpContext httpContext);
}
