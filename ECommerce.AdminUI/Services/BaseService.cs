using Microsoft.AspNetCore.Http;

namespace ECommerce.AdminUI.Services
{
    public abstract class BaseService
    {
        protected readonly IHttpContextAccessor HttpContextAccessor;
        protected readonly AuthService AuthService;
        protected readonly ILogger Logger;

        protected BaseService(
            IHttpContextAccessor httpContextAccessor,
            AuthService authService,
            ILogger logger)
        {
            HttpContextAccessor = httpContextAccessor;
            AuthService = authService;
            Logger = logger;
        }

        protected string? GetTokenFromSession()
        {
            return HttpContextAccessor.HttpContext?.Session.GetString("AuthToken");
        }

        protected string? GetUsernameFromSession()
        {
            return HttpContextAccessor.HttpContext?.Session.GetString("Username");
        }

        protected void AddAuthorizationHeader(HttpClient httpClient, string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Remove("Authorization");
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            }
        }
    }
}
