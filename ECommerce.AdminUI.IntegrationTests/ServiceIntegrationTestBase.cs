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
using Xunit.Abstractions;

namespace ECommerce.AdminUI.IntegrationTests;

/// <summary>
/// Base class for service integration tests that provides common setup functionality
/// </summary>
public abstract class ServiceIntegrationTestBase
{
    protected readonly ITestOutputHelper TestOutputHelper;

    protected ServiceIntegrationTestBase(ITestOutputHelper testOutputHelper)
    {
        TestOutputHelper = testOutputHelper;
    }

    /// <summary>
    /// Creates a mock HttpClient that returns a predefined response
    /// </summary>
    protected HttpClient CreateMockHttpClient(HttpStatusCode statusCode, string content)
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

    /// <summary>
    /// Sets up common services used by all integration tests
    /// </summary>
    protected IServiceCollection SetupCommonServices(HttpClient httpClient, bool setupAuthSuccess = false)
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
            .Returns((string _, out byte[] value) => { value = Encoding.UTF8.GetBytes("test-token"); return true; });
        sessionMock.Setup(x => x.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
            .Returns((string _, out byte[] value) => { value = Encoding.UTF8.GetBytes("testuser"); return true; });

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

        var orderServiceMock = new Mock<IOrderService>();
        services.AddSingleton(orderServiceMock.Object);

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

        // Derived classes will add their specific service
        ConfigureServices(services);

        return services;
    }

    /// <summary>
    /// Configure service-specific dependencies
    /// </summary>
    protected abstract void ConfigureServices(IServiceCollection services);
}
