using System.Text;
using ECommerce.BusinessEvents;
using ECommerce.BusinessEvents.Endpoints;
using ECommerce.Modules.Customers;
using ECommerce.Modules.Customers.Endpoints;
using ECommerce.Modules.Orders;
using ECommerce.Modules.Orders.Endpoints;
using ECommerce.Modules.Products;
using ECommerce.Modules.Products.Endpoints;
using ECommerceApp.Endpoints;
using ECommerceApp.Services;
using ECommerceApp.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace ECommerceApp;

public partial class Program
{
    public static async Task  Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Log all environment variables to help with debugging
        Console.WriteLine("=== Environment Variables ===");
        Console.WriteLine($"ASPNETCORE_ENVIRONMENT: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "not set"}");
        Console.WriteLine($"OTEL_EXPORTER_OTLP_ENDPOINT: {Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT") ?? "not set"}");
        Console.WriteLine("==========================");
        // test
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new() { Title = "E-Commerce API", Version = "v1" });
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Name = "Authorization",
                Description = "Bearer Authentication with JWT token",
                Type = SecuritySchemeType.Http
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    []
                }
            });
        });

        // Add OpenTelemetry
        builder.Services.AddOpenTelemetryConfiguration(builder.Configuration);

        // Register services
        builder.Services.AddOrderModule(builder.Configuration);
        builder.Services.AddCustomerModule(builder.Configuration);
        builder.Services.AddProductModule(builder.Configuration);
        builder.Services.AddBusinessEventsModule(builder.Configuration);
        builder.Services.AddScoped<IAdminService, AdminService>();
        builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"] ?? string.Empty))
                };
            });

        builder.Services.AddAuthorization();

        var app = builder.Build();
        var logger = app.Logger;

        // Configure Prometheus endpoint if enabled
        var openTelemetryConfig = builder.Configuration.GetSection("OpenTelemetry");
        if (openTelemetryConfig.GetValue<bool>("Exporters:Prometheus:Enabled"))
        {
            logger.LogInformation("Prometheus exporter is enabled. Configuring scraping endpoint");
            app.MapPrometheusScrapingEndpoint();
        }

        // Log ASPNETCORE_PATHBASE value at startup
        var pathBase = app.Configuration["ASPNETCORE_PATHBASE"] ?? Environment.GetEnvironmentVariable("ASPNETCORE_PATHBASE");
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

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                if (string.IsNullOrEmpty(pathBase))
                {
                    return;
                }

                // Adjust Swagger endpoint when using a path base
                string swaggerJsonPath = string.IsNullOrEmpty(pathBase) ? "/swagger/v1/swagger.json" : $"{pathBase}/swagger/v1/swagger.json";
                options.SwaggerEndpoint(swaggerJsonPath, "E-Commerce API V1");
            });
        }

        // After builder.Build():
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapGet("/", () => "E-Commerce Modular Monolith with .NET 8!").WithTags("Home");
        app.MapOrderEndpoints();
        app.MapCustomerEndpoints();
        app.MapProductEndpoints();
        app.MapBusinessEventEndpoints();
        app.MapAdminEndpoints();

        app.Run();
    }
}
