using ECommerce.AdminUI;
using Microsoft.AspNetCore.DataProtection;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Log all environment variables to help with debugging
Console.WriteLine("=== Environment Variables ===");
Console.WriteLine($"ModularMonolithApiUrl: {Environment.GetEnvironmentVariable("ModularMonolithApiUrl") ?? "not set"}");
Console.WriteLine($"TokenServiceUrl: {Environment.GetEnvironmentVariable("TokenServiceUrl") ?? "not set"}");
Console.WriteLine($"ASPNETCORE_ENVIRONMENT: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "not set"}");
Console.WriteLine("==========================");

// Log configuration values to help with debugging
Console.WriteLine("=== Configuration Values ===");
Console.WriteLine($"ModularMonolithApiUrl from config: {builder.Configuration["ModularMonolithApiUrl"] ?? "not set"}");
Console.WriteLine($"TokenServiceUrl from config: {builder.Configuration["TokenServiceUrl"] ?? "not set"}");
Console.WriteLine("==========================");

// Add OpenTelemetry
builder.Services.AddOpenTelemetryConfiguration(builder.Configuration);

// Add Prometheus metrics
builder.Services.AddHealthChecks();

// Add services to the container.
builder.Services.AddRazorPages()
    .AddMvcOptions(options =>
    {
        options.Filters.Add<ECommerce.AdminUI.Filters.AuthenticationFilter>();
    });

// Add session state
// builder.Services.AddDistributedMemoryCache();
// Use Redis for distributed session state instead of in-memory cache
string redisConnectionString = builder.Configuration["REDIS_CONNECTION_STRING"] ?? Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING");
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "ECommerce.AdminUI.Session:";
});

// Persist Data Protection keys to Redis for multi-pod/ingress scenarios
builder.Services.AddDataProtection()
    .PersistKeysToStackExchangeRedis(ConnectionMultiplexer.Connect(redisConnectionString), "ECommerce.AdminUI-Keys")
    .SetApplicationName("ECommerce.AdminUI");

// Configure antiforgery to use root path for cookies - consistent with session cookie
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Path = "/"; // Set to root path for consistency with session cookie
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});



// Add HttpContextAccessor for accessing session in services
builder.Services.AddHttpContextAccessor();

// Add HTTP clients for communication with modular monolith API
builder.Services.AddHttpClient("ModularMonolith", client =>
{
    // Ensure base URL ends with a trailing slash for proper URL resolution
    var baseUrl = builder.Configuration["ModularMonolithApiUrl"] ?? "http://localhost:56000/modulith";
    if (!baseUrl.EndsWith("/"))
    {
        baseUrl += "/";
    }
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("X-Correlation-Id", Guid.NewGuid().ToString());

    // Log the configured base address for debugging
    Console.WriteLine($"ModularMonolith API base address: {baseUrl}");
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    return new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback =
            (_, _, _, _) => true // For development only!
    };
});

// Add HTTP client for token generation
builder.Services.AddHttpClient("TokenService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["TokenServiceUrl"] ?? "http://localhost:56000");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    return new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback =
            (_, _, _, _) => true // For development only!
    };
});

// Register application services
builder.Services.AddScoped<ECommerce.AdminUI.Services.IAuthService, ECommerce.AdminUI.Services.AuthService>();
builder.Services.AddScoped<ECommerce.AdminUI.Services.IOrderService, ECommerce.AdminUI.Services.OrderService>();
builder.Services.AddScoped<ECommerce.AdminUI.Services.ICustomerService, ECommerce.AdminUI.Services.CustomerService>();
builder.Services.AddScoped<ECommerce.AdminUI.Services.IProductService, ECommerce.AdminUI.Services.ProductService>();
builder.Services.AddScoped<ECommerce.AdminUI.Services.DashboardService>();

// Configure session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Path = "/"; // Set to root to allow session sharing across applications
    options.Cookie.SameSite = SameSiteMode.Lax; // Allow redirects to properly maintain the session
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Respect the request scheme
});

var app = builder.Build();
var logger = app.Logger;

// Log ASPNETCORE_PATHBASE value at startup
string? pathBase = app.Configuration["ASPNETCORE_PATHBASE"] ?? Environment.GetEnvironmentVariable("ASPNETCORE_PATHBASE");
logger.LogInformation("ASPNETCORE_PATHBASE is set to: \'{PathBase}\'", pathBase);

// Configure Prometheus endpoint if enabled - before any PathBase middleware
var openTelemetryConfig = builder.Configuration.GetSection("OpenTelemetry");
if (openTelemetryConfig.GetValue<bool>("Exporters:Prometheus:Enabled"))
{
    logger.LogInformation("Prometheus exporter is enabled. Configuring scraping endpoint at ROOT path (/metrics)");
    // Branch the pipeline for /metrics before PathBase is applied
    app.MapWhen(ctx => ctx.Request.Path.Equals("/metrics", StringComparison.OrdinalIgnoreCase), metricsApp =>
    {
        metricsApp.UseRouting();
        metricsApp.UseEndpoints(endpoints =>
        {
            endpoints.MapPrometheusScrapingEndpoint();
        });
    });
}

// Apply the path base if it's set
if (!string.IsNullOrEmpty(pathBase))
{
    app.UsePathBase(pathBase);
    app.Use((context, next) =>
    {
        context.Request.PathBase = pathBase;
        return next();
    });
    logger.LogInformation("Path base applied: {PathBase}", pathBase);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Add session middleware to enable session state
app.UseSession();

// Add Content Security Policy (CSP) header middleware
app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        // Allow scripts and styles from cdn.jsdelivr.net with specific paths
        context.Response.Headers["Content-Security-Policy"] =
            "default-src 'self'; " +
            "script-src 'self' https://cdn.jsdelivr.net https://cdn.jsdelivr.com 'unsafe-inline'; " +
            "style-src 'self' https://cdn.jsdelivr.net https://cdn.jsdelivr.com 'unsafe-inline'; " + // Added 'unsafe-inline' for inline styles
            "img-src 'self' data:; " +
            "font-src 'self' https://cdn.jsdelivr.net https://cdn.jsdelivr.com; " + // Added font-src for Bootstrap Icons
            "connect-src 'self'; " +
            "object-src 'none'; " +
            "frame-ancestors 'none'; " +
            "base-uri 'self'; " +
            "form-action 'self'";
        // Add X-Content-Type-Options header to prevent MIME sniffing
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        return Task.CompletedTask;
    });
    await next();
});

app.UseRouting();
app.UseAuthorization();

app.MapRazorPages();
app.MapHealthChecks("/health");

app.Run();
