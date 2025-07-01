using System;
using System.Net;
using System.Threading.Tasks;
using ECommerce.AdminUI.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace ECommerce.AdminUI.IntegrationTests;

public class ProductServiceIntegrationTests : ServiceIntegrationTestBase
{
    public ProductServiceIntegrationTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<IProductService, ProductService>();
    }

    [Fact]
    public async Task GetAllProductsAsync_ReturnsStubbedProducts()
    {
        // Arrange
        var productsJson = "[]";
        var httpClient = CreateMockHttpClient(HttpStatusCode.OK, productsJson);
        var services = SetupCommonServices(httpClient);
        var provider = services.BuildServiceProvider();
        var productService = provider.GetRequiredService<IProductService>();

        // Act
        var products = await productService.GetAllProductsAsync();

        // Assert
        Assert.NotNull(products);
        Assert.Empty(products);
    }

    [Fact]
    public async Task CreateProductAsync_ReturnsTrue_WhenApiReturnsSuccess()
    {
        // Arrange
        var httpClient = CreateMockHttpClient(HttpStatusCode.OK, "true");
        var services = SetupCommonServices(httpClient, setupAuthSuccess: true);
        var provider = services.BuildServiceProvider();
        var productService = provider.GetRequiredService<IProductService>();

        var product = new ProductDto
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Description = "A test product",
            Price = 9.99m
        };

        // Act
        var result = await productService.CreateProductAsync(product);

        // Log the result for debugging
        if (!result)
        {
            var requestJson = System.Text.Json.JsonSerializer.Serialize(product);
            TestOutputHelper.WriteLine($"Product payload: {requestJson}");
            TestOutputHelper.WriteLine("Stubbed HTTP response: 200 OK, body: 'true'");
        }

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task UpdateProductAsync_ReturnsTrue_WhenApiReturnsSuccess()
    {
        // Arrange
        var httpClient = CreateMockHttpClient(HttpStatusCode.OK, "true");
        var services = SetupCommonServices(httpClient, setupAuthSuccess: true);
        var provider = services.BuildServiceProvider();
        var productService = provider.GetRequiredService<IProductService>();

        var product = new ProductDto
        {
            Id = Guid.NewGuid(),
            Name = "Updated Product",
            Description = "Updated description",
            Price = 19.99m
        };

        // Act
        var result = await productService.UpdateProductAsync(product.Id, product);

        // Log the result for debugging
        if (!result)
        {
            var requestJson = System.Text.Json.JsonSerializer.Serialize(product);
            TestOutputHelper.WriteLine($"Product payload: {requestJson}");
            TestOutputHelper.WriteLine("Stubbed HTTP response: 200 OK, body: 'true'");
        }

        // Assert
        Assert.True(result);
    }
}
