var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddScoped<ECommerce.AdminUI.Services.AuthService>();
builder.Services.AddScoped<ECommerce.AdminUI.Services.DashboardService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession(); // Enable session middleware

app.UseAuthorization();

app.MapRazorPages();

app.Run();
