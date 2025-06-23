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
using Prometheus;

namespace ECommerceApp;

public partial class Program
{
    public static async Task  Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
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

        // Enable Prometheus metrics
        app.UseHttpMetrics();

        // Log ASPNETCORE_PATHBASE value at startup
        var logger = app.Logger;
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

        // Expose /metrics endpoint for Prometheus
        app.MapMetrics();

        await BusinessEventsModule.InitializeDefaultSchemasAsync(app.Services);

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
