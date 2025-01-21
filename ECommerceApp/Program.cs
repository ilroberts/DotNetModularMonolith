using System.Text;
using ECommerce.Modules.Orders;
using ECommerce.Modules.Customers;
using ECommerce.Modules.Products;
using ECommerce.Modules.Orders.Endpoints;
using ECommerce.Modules.Customers.Endpoints;
using ECommerce.Modules.Products.Endpoints;
using ECommerceApp.Endpoints;
using ECommerceApp.Services;
using ECommerceApp.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

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
            Array.Empty<string>()
        }
    });
});

// Register services
builder.Services.AddOrderModule(builder.Configuration);
builder.Services.AddCustomerModule(builder.Configuration);
builder.Services.AddProductModule(builder.Configuration);
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// After builder.Build():
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "E-Commerce Modular Monolith with .NET 8!").WithTags("Home");
app.MapOrderEndpoints();
app.MapCustomerEndpoints();
app.MapProductEndpoints();
app.MapAdminEndpoints();

app.Run();
