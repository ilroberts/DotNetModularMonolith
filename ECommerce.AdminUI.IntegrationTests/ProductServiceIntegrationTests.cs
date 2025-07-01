using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.AdminUI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using Xunit;
using Xunit.Abstractions;

namespace ECommerce.AdminUI.IntegrationTests;

public class ProductServiceIntegrationTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public ProductServiceIntegrationTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    private HttpClient CreateMockHttpClient(HttpStatusCode statusCode, string content)
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content),
            });

        return new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://fake-api/")
        };
    }

    private IServiceCollection SetupCommonServices(HttpClient httpClient, bool setupAuthSuccess = false)
    {
        var services = new ServiceCollection();
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);
        services.AddSingleton(httpClientFactoryMock.Object);
        services.AddLogging();

        var sessionMock = new Mock<ISession>();
        sessionMock.Setup(x => x.TryGetValue("AuthToken", out It.Ref<byte[]>.IsAny))
            .Returns((string _, out byte[] value) => { value = Encoding.UTF8.GetBytes("test-token"); return true; });
        sessionMock.Setup(x => x.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
            .Returns((string _, out byte[] value) => { value = Encoding.UTF8.GetBytes("testuser"); return true; });

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(x => x.Session).Returns(sessionMock.Object);

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContextMock.Object);
        services.AddSingleton(httpContextAccessorMock.Object);

        var orderServiceMock = new Mock<IOrderService>();
        services.AddSingleton(orderServiceMock.Object);
        var customerServiceMock = new Mock<ICustomerService>();
        services.AddSingleton(customerServiceMock.Object);
        var authServiceMock = new Mock<IAuthService>();
        if (setupAuthSuccess)
        {
            authServiceMock
                .Setup(x => x.ExecuteWithTokenRefreshAsync(
                    It.IsAny<Func<string, Task<HttpResponseMessage>>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<HttpContext>()))
                .ReturnsAsync((true, new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("true"),
                }));
        }
        services.AddSingleton(authServiceMock.Object);
        services.AddTransient<IProductService, ProductService>();
        return services;
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
            _testOutputHelper.WriteLine($"Product payload: {requestJson}");
            _testOutputHelper.WriteLine("Stubbed HTTP response: 200 OK, body: 'true'");
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
            _testOutputHelper.WriteLine($"Product payload: {requestJson}");
            _testOutputHelper.WriteLine("Stubbed HTTP response: 200 OK, body: 'true'");
        }

        // Assert
        Assert.True(result);
    }
}

