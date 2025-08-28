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
    private readonly string _productsDbName;
    private readonly string _customersDbName;
    private readonly string _ordersDbName;
    private readonly string _businessEventsDbName;

    public CustomWebApplicationFactory(string connectionString)
    {
        _connectionString = connectionString;
        // Generate unique DB names per factory instance
        _productsDbName = $"InMemoryProductsTestDb_{Guid.NewGuid()}";
        _customersDbName = $"InMemoryCustomersTestDb_{Guid.NewGuid()}";
        _ordersDbName = $"InMemoryOrdersTestDb_{Guid.NewGuid()}";
        _businessEventsDbName = $"InMemoryBusinessEventsTestDb_{Guid.NewGuid()}";
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
                options.UseInMemoryDatabase(_productsDbName);
            });

            // Remove and add in-memory for CustomerDbContext
            var customerDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ECommerce.Modules.Customers.Persistence.CustomerDbContext>));
            if (customerDescriptor != null)
            {
                services.Remove(customerDescriptor);
            }
            services.AddDbContext<ECommerce.Modules.Customers.Persistence.CustomerDbContext>(options =>
            {
                options.UseInMemoryDatabase(_customersDbName);
            });

            // Remove and add in-memory for OrderDbContext
            var orderDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ECommerce.Modules.Orders.Persistence.OrderDbContext>));
            if (orderDescriptor != null)
            {
                services.Remove(orderDescriptor);
            }
            services.AddDbContext<ECommerce.Modules.Orders.Persistence.OrderDbContext>(options =>
            {
                options.UseInMemoryDatabase(_ordersDbName);
            });

            // Remove and add in-memory for BusinessEventDbContext
            var businessEventDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ECommerce.BusinessEvents.Persistence.BusinessEventDbContext>));
            if (businessEventDescriptor != null)
            {
                services.Remove(businessEventDescriptor);
            }
            services.AddDbContext<ECommerce.BusinessEvents.Persistence.BusinessEventDbContext>(options =>
            {
                options.UseInMemoryDatabase(_businessEventsDbName);
            });

            // Seed schemas for BusinessEventDbContext only once
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ECommerce.BusinessEvents.Persistence.BusinessEventDbContext>();
            db.Database.EnsureCreated();
            if (db.SchemaVersions.Any())
            {
                return;
            }

            db.SchemaVersions.AddRange(ECommerce.BusinessEvents.Domain.Schemas.CustomerSchemaVersions.All);
            db.SchemaVersions.AddRange(ECommerce.BusinessEvents.Domain.Schemas.ProductSchemaVersions.All);
            db.SaveChanges();
        });

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            var inMemorySettings = new Dictionary<string, string>
            {
                {"ConnectionStrings:DefaultConnection", _connectionString}
            };
            configBuilder.AddInMemoryCollection(inMemorySettings!);
        });
    }
}
