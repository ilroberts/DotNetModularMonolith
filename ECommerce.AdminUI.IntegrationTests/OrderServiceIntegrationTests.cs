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

public class OrderServiceIntegrationTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public OrderServiceIntegrationTests(ITestOutputHelper testOutputHelper)
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

        // Setup HttpClientFactory
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);
        services.AddSingleton(httpClientFactoryMock.Object);
        services.AddLogging();

        // Setup session and HttpContext
        var sessionMock = new Mock<ISession>();
        sessionMock.Setup(x => x.TryGetValue("AuthToken", out It.Ref<byte[]>.IsAny))
            .Returns((string key, out byte[] value) => { value = Encoding.UTF8.GetBytes("test-token"); return true; });
        sessionMock.Setup(x => x.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
            .Returns((string key, out byte[] value) => { value = Encoding.UTF8.GetBytes("testuser"); return true; });

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(x => x.Session).Returns(sessionMock.Object);

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContextMock.Object);
        services.AddSingleton(httpContextAccessorMock.Object);

        // Setup dependent services
        var customerServiceMock = new Mock<ICustomerService>();
        services.AddSingleton(customerServiceMock.Object);

        var productServiceMock = new Mock<IProductService>();
        services.AddSingleton(productServiceMock.Object);

        var authServiceMock = new Mock<IAuthService>();

        if (setupAuthSuccess)
        {
            // Mock ExecuteWithTokenRefreshAsync to always return success for tests that need it
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
        services.AddTransient<IOrderService, OrderService>();

        return services;
    }

    [Fact]
    public async Task GetOrdersAsync_ReturnsStubbedOrders()
    {
        // Arrange
        var httpClient = CreateMockHttpClient(HttpStatusCode.OK, "[]");
        var services = SetupCommonServices(httpClient);
        var provider = services.BuildServiceProvider();
        var orderService = provider.GetRequiredService<IOrderService>();

        // Act
        var orders = await orderService.GetAllOrdersAsync();

        // Assert
        Assert.NotNull(orders);
        Assert.Empty(orders);
    }

    [Fact]
    public async Task CreateOrderAsync_ReturnsTrue_WhenApiReturnsSuccess()
    {
        // Arrange
        var httpClient = CreateMockHttpClient(HttpStatusCode.OK, "true");
        var services = SetupCommonServices(httpClient, setupAuthSuccess: true);
        var provider = services.BuildServiceProvider();
        var orderService = provider.GetRequiredService<IOrderService>();

        var order = new OrderDto
        {
            Id = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            CustomerName = "Test Customer",
            CustomerEmail = "test@example.com",
            ProductId = Guid.NewGuid(),
            ProductName = "Test Product",
            ProductPrice = 10.0m,
            Quantity = 2,
            TotalPrice = 20.0m,
            CreatedAt = DateTime.UtcNow,
            Status = "Created",
            OrderDate = DateTime.UtcNow,
            OrderStatus = "Pending"
        };

        // Act
        var result = await orderService.CreateOrderAsync(order);

        // Log the result for debugging
        if (!result)
        {
            var requestJson = System.Text.Json.JsonSerializer.Serialize(order);
            _testOutputHelper.WriteLine($"Order payload: {requestJson}");
            _testOutputHelper.WriteLine("Stubbed HTTP response: 200 OK, body: 'true'");
        }

        // Assert
        Assert.True(result);
    }
}
