namespace ECommerce.AdminUI.Services
{
    public abstract class BaseService(
        IHttpContextAccessor httpContextAccessor,
        IAuthService authService,
        ILogger logger)
    {
        protected readonly IHttpContextAccessor HttpContextAccessor = httpContextAccessor;
        protected readonly IAuthService AuthService = authService;
        protected readonly ILogger Logger = logger;

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
            if (string.IsNullOrEmpty(token))
            {
                return;
            }

            httpClient.DefaultRequestHeaders.Remove("Authorization");
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        }
    }
}
