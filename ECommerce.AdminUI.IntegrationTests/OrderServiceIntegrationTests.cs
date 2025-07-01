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

    [Fact]
    public async Task GetOrdersAsync_ReturnsStubbedOrders()
    {
        // Arrange: stub HTTP response
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("[]"), // Return empty list for simplicity
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://fake-api/")
        };

        // Mock IHttpClientFactory to return our stubbed HttpClient
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        var services = new ServiceCollection();
        services.AddSingleton(httpClientFactoryMock.Object);
        services.AddLogging();
        // Mock session to return a fake token and username via TryGetValue
        var sessionMock = new Mock<ISession>();
        sessionMock.Setup(x => x.TryGetValue("AuthToken", out It.Ref<byte[]>.IsAny))
            .Returns((string key, out byte[] value) => { value = Encoding.UTF8.GetBytes("test-token"); return true; });
        sessionMock.Setup(x => x.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
            .Returns((string key, out byte[] value) => { value = Encoding.UTF8.GetBytes("testuser"); return true; });

        // Mock HttpContext to use the mocked session
        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(x => x.Session).Returns(sessionMock.Object);

        // Mock IHttpContextAccessor to return the mocked HttpContext
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContextMock.Object);
        services.AddSingleton(httpContextAccessorMock.Object);
        // Add a mock for ICustomerService since OrderService depends on it
        var customerServiceMock = new Mock<ICustomerService>();
        services.AddSingleton(customerServiceMock.Object);
        // Add a mock for IProductService since OrderService may depend on it
        var productServiceMock = new Mock<IProductService>();
        services.AddSingleton(productServiceMock.Object);
        // Add a mock for IAuthService since OrderService or dependencies may require it
        var authServiceMock = new Mock<IAuthService>();
        services.AddSingleton(authServiceMock.Object);
        services.AddTransient<IOrderService, OrderService>();
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
        // Arrange: stub HTTP response for order creation
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("true"),
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new System.Uri("https://fake-api/")
        };

        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        var services = new ServiceCollection();
        services.AddSingleton(httpClientFactoryMock.Object);
        services.AddLogging();
        // Mock session to return a fake token and username via TryGetValue
        var sessionMock = new Mock<Microsoft.AspNetCore.Http.ISession>();
        sessionMock.Setup(x => x.TryGetValue("AuthToken", out It.Ref<byte[]>.IsAny))
            .Returns((string key, out byte[] value) => { value = System.Text.Encoding.UTF8.GetBytes("test-token"); return true; });
        sessionMock.Setup(x => x.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
            .Returns((string key, out byte[] value) => { value = System.Text.Encoding.UTF8.GetBytes("testuser"); return true; });

        // Mock HttpContext to use the mocked session
        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(x => x.Session).Returns(sessionMock.Object);

        // Mock IHttpContextAccessor to return the mocked HttpContext
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContextMock.Object);
        services.AddSingleton(httpContextAccessorMock.Object);
        var customerServiceMock = new Mock<ICustomerService>();
        services.AddSingleton(customerServiceMock.Object);
        var productServiceMock = new Mock<IProductService>();
        services.AddSingleton(productServiceMock.Object);
        var authServiceMock = new Mock<IAuthService>();
        // Mock ExecuteWithTokenRefreshAsync to always return (true, a successful response)
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
        services.AddSingleton(authServiceMock.Object);
        services.AddTransient<IOrderService, OrderService>();
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
            // Try to log the request payload and stubbed response
            var requestJson = System.Text.Json.JsonSerializer.Serialize(order);
            _testOutputHelper.WriteLine($"Order payload: {requestJson}");
            _testOutputHelper.WriteLine("Stubbed HTTP response: 200 OK, body: 'true'");
        }

        // Assert
        Assert.True(result);
    }
}
