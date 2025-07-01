// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerceApp.IntegrationTests;

public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup>
    where TStartup : class
{
    private readonly string _connectionString;

    public CustomWebApplicationFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing ProductDbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ECommerce.Modules.Products.Persistence.ProductDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }
            // Add in-memory database for ProductDbContext
            services.AddDbContext<ECommerce.Modules.Products.Persistence.ProductDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryProductsTestDb");
            });
            // Repeat for other module DbContexts as needed
        });
        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
            var inMemorySettings = new Dictionary<string, string>
            {
                {"ConnectionStrings:DefaultConnection", _connectionString}
            };
            configBuilder.AddInMemoryCollection(inMemorySettings!);
        });
    }
}
