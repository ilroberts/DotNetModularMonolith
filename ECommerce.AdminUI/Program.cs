using Microsoft.AspNetCore.DataProtection;
using Prometheus;
using OpenTelemetry.Metrics;

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

// Add OpenTelemetry with ASP.NET Core built-in metrics
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation();
        metrics.AddHttpClientInstrumentation();
        metrics.AddPrometheusExporter();
    });

// Add Prometheus metrics
builder.Services.AddHealthChecks();

// Add services to the container.
builder.Services.AddRazorPages()
    .AddMvcOptions(options =>
    {
        options.Filters.Add<ECommerce.AdminUI.Filters.AuthenticationFilter>();
    });

// Add session state
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Path = Environment.GetEnvironmentVariable("ASPNETCORE_PATHBASE") ?? "/";
});

// Persist Data Protection keys to a shared volume for multi-pod/ingress scenarios
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/var/dpkeys"))
    .SetApplicationName("ECommerce.AdminUI");

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

    // Log the configured base address for debugging
    Console.WriteLine($"ModularMonolith API base address: {baseUrl}");
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    return new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback =
            (sender, cert, chain, sslPolicyErrors) => true // For development only!
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
            (sender, cert, chain, sslPolicyErrors) => true // For development only!
    };
});

// Register application services
builder.Services.AddScoped<ECommerce.AdminUI.Services.CustomerService>();
builder.Services.AddScoped<ECommerce.AdminUI.Services.ProductService>();
builder.Services.AddScoped<ECommerce.AdminUI.Services.AuthService>();
builder.Services.AddScoped<ECommerce.AdminUI.Services.DashboardService>();
builder.Services.AddScoped<ECommerce.AdminUI.Services.OrderService>();

var app = builder.Build();
var logger = app.Logger;

// Log ASPNETCORE_PATHBASE value at startup
string? pathBase = app.Configuration["ASPNETCORE_PATHBASE"] ?? Environment.GetEnvironmentVariable("ASPNETCORE_PATHBASE");
logger.LogInformation("ASPNETCORE_PATHBASE is set to: \'{PathBase}\'", pathBase);

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

// Add Prometheus metrics endpoint
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession(); // Enable session middleware

app.UseAuthorization();

// Configure metrics endpoint before mapping controllers
app.UseMetricServer("/metrics"); // This exposes the metrics endpoint
app.UseHttpMetrics(); // Collect HTTP metrics

app.MapRazorPages();
app.MapHealthChecks("/health");

app.Run();
