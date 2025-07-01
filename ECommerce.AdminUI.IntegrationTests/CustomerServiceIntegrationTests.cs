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

public class CustomerServiceIntegrationTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public CustomerServiceIntegrationTests(ITestOutputHelper testOutputHelper)
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
            .Returns((string key, out byte[] value) => { value = Encoding.UTF8.GetBytes("test-token"); return true; });
        sessionMock.Setup(x => x.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
            .Returns((string key, out byte[] value) => { value = Encoding.UTF8.GetBytes("testuser"); return true; });

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(x => x.Session).Returns(sessionMock.Object);

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContextMock.Object);
        services.AddSingleton(httpContextAccessorMock.Object);

        var orderServiceMock = new Mock<IOrderService>();
        services.AddSingleton(orderServiceMock.Object);
        var productServiceMock = new Mock<IProductService>();
        services.AddSingleton(productServiceMock.Object);
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
        services.AddTransient<ICustomerService, CustomerService>();
        return services;
    }

    [Fact]
    public async Task GetAllCustomersAsync_ReturnsStubbedCustomers()
    {
        // Arrange
        var customersJson = "[]";
        var httpClient = CreateMockHttpClient(HttpStatusCode.OK, customersJson);
        var services = SetupCommonServices(httpClient);
        var provider = services.BuildServiceProvider();
        var customerService = provider.GetRequiredService<ICustomerService>();

        // Act
        var customers = await customerService.GetAllCustomersAsync();

        // Assert
        Assert.NotNull(customers);
        Assert.Empty(customers);
    }

    [Fact]
    public async Task CreateCustomerAsync_ReturnsTrue_WhenApiReturnsSuccess()
    {
        // Arrange
        var httpClient = CreateMockHttpClient(HttpStatusCode.OK, "true");
        var services = SetupCommonServices(httpClient, setupAuthSuccess: true);
        var provider = services.BuildServiceProvider();
        var customerService = provider.GetRequiredService<ICustomerService>();

        var customer = new CustomerDto
        {
            Id = Guid.NewGuid(),
            Name = "Test Customer",
            Email = "test@example.com"
        };

        // Act
        var result = await customerService.CreateCustomerAsync(customer);

        // Log the result for debugging
        if (!result)
        {
            var requestJson = System.Text.Json.JsonSerializer.Serialize(customer);
            _testOutputHelper.WriteLine($"Customer payload: {requestJson}");
            _testOutputHelper.WriteLine("Stubbed HTTP response: 200 OK, body: 'true'");
        }

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task UpdateCustomerAsync_ReturnsTrue_WhenApiReturnsSuccess()
    {
        // Arrange
        var httpClient = CreateMockHttpClient(HttpStatusCode.OK, "true");
        var services = SetupCommonServices(httpClient, setupAuthSuccess: true);
        var provider = services.BuildServiceProvider();
        var customerService = provider.GetRequiredService<ICustomerService>();

        var customer = new CustomerDto
        {
            Id = Guid.NewGuid(),
            Name = "Updated Customer",
            Email = "updated@example.com"
        };

        // Act
        var result = await customerService.UpdateCustomerAsync(customer.Id, customer);

        // Log the result for debugging
        if (!result)
        {
            var requestJson = System.Text.Json.JsonSerializer.Serialize(customer);
            _testOutputHelper.WriteLine($"Customer payload: {requestJson}");
            _testOutputHelper.WriteLine("Stubbed HTTP response: 200 OK, body: 'true'");
        }

        // Assert
        Assert.True(result);
    }
}
