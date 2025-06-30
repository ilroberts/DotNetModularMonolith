using ECommerce.AdminUI.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using System.Text.Json;

namespace ECommerce.AdminUI.Tests.Services
{
    // Define delegate for TryGetValue callbacks
    delegate void TryGetValueCallback(string key, out byte[] value);

    public class DashboardServiceTests
    {
        private readonly DashboardService _dashboardService;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<ILogger<DashboardService>> _loggerMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<IOrderService> _orderServiceMock; // Changed to IOrderService
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;
        private readonly DefaultHttpContext _httpContext;
        private readonly Mock<ISession> _sessionMock;

        public DashboardServiceTests()
        {
            // Set up HTTP message handler mock
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("http://test-api.com/")
            };

            // Set up HTTP client factory mock
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _httpClientFactoryMock
                .Setup(x => x.CreateClient("ModularMonolith"))
                .Returns(_httpClient);

            // Set up logger mock
            _loggerMock = new Mock<ILogger<DashboardService>>();

            // Set up session mock
            _sessionMock = new Mock<ISession>();
            var sessionDict = new Dictionary<string, byte[]>();

            // Setup Set method directly instead of using the extension method
            _sessionMock.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
                .Callback<string, byte[]>((key, value) => sessionDict[key] = value);

            // Setup TryGetValue to retrieve the string values - fixed out parameter handling
            _sessionMock.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny!))
                .Callback(new TryGetValueCallback((string key, out byte[] value) =>
                {
                    value = sessionDict.TryGetValue(key, out byte[]? bytes) ? bytes : null!;
                }))
                .Returns((string key, byte[] value) => sessionDict.ContainsKey(key));

            // No default authorization token setup here - let each test set up its own token if needed

            // Set up HTTP context with session
            _httpContext = new DefaultHttpContext
            {
                Session = _sessionMock.Object
            };

            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(_httpContext);

            // Create proper mocks for CustomerService and ProductService with their required parameters
            var customerServiceMock = new Mock<CustomerService>(
                _httpClientFactoryMock.Object,
                Mock.Of<ILogger<CustomerService>>(),
                _httpContextAccessorMock.Object,
                Mock.Of<IAuthService>()
            );

            var productServiceMock = new Mock<ProductService>(
                _httpClientFactoryMock.Object,
                Mock.Of<ILogger<ProductService>>(),
                _httpContextAccessorMock.Object,
                Mock.Of<IAuthService>()
            );

            // Set up order service mock - use interface instead of concrete class
            _orderServiceMock = new Mock<IOrderService>();

            // Create the service under test
            _dashboardService = new DashboardService(
                _httpClientFactoryMock.Object,
                _loggerMock.Object,
                _httpContextAccessorMock.Object,
                _orderServiceMock.Object
            );
        }

        [Fact]
        public async Task GetDashboardStatsAsync_ReturnsCompleteStats_WhenAllApiCallsSucceed()
        {
            // Arrange
            // Setup auth token in session
            var tokenBytes = Encoding.UTF8.GetBytes("valid-token");
            _sessionMock.Setup(s => s.TryGetValue("AuthToken", out It.Ref<byte[]>.IsAny!))
                .Callback(new TryGetValueCallback((string key, out byte[] value) => {
                    value = tokenBytes;
                }))
                .Returns(true);

            // Setup mock customer response
            var customers = new List<CustomerDto>
            {
                new() { Id = Guid.NewGuid(), Name = "Customer 1" },
                new() { Id = Guid.NewGuid(), Name = "Customer 2" },
                new() { Id = Guid.NewGuid(), Name = "Customer 3" }
            };

            // Setup mock product response
            var products = new List<ProductDto>
            {
                new() { Id = Guid.NewGuid(), Name = "Product 1", Price = 10.99m },
                new() { Id = Guid.NewGuid(), Name = "Product 2", Price = 20.49m }
            };

            // Setup mock order response
            var orders = new List<OrderDto>
            {
                new() { Id = Guid.NewGuid(), CustomerId = Guid.NewGuid(), TotalPrice = 30.99m },
                new() { Id = Guid.NewGuid(), CustomerId = Guid.NewGuid(), TotalPrice = 45.50m },
                new() { Id = Guid.NewGuid(), CustomerId = Guid.NewGuid(), TotalPrice = 15.25m }
            };

            // Setup HTTP responses
            SetupHttpResponse("/customers", HttpStatusCode.OK, JsonSerializer.Serialize(customers));
            SetupHttpResponse("/products", HttpStatusCode.OK, JsonSerializer.Serialize(products));

            // Setup OrderService mock to return orders
            _orderServiceMock.Setup(x => x.GetAllOrdersAsync())
                .ReturnsAsync(orders);

            // Act
            var result = await _dashboardService.GetDashboardStatsAsync();

            // Assert
            result.Should().NotBeNull();
            result.CustomerCount.Should().Be(3);
            result.ProductCount.Should().Be(2);
            result.OrderCount.Should().Be(3);
            result.TotalSales.Should().Be(91.74m);

            // Verify HTTP calls had authorization header
            VerifyAuthorizationHeader("Bearer valid-token");
        }

        [Fact]
        public async Task GetDashboardStatsAsync_ReturnsPartialStats_WhenSomeApiCallsFail()
        {
            // Arrange
            // Setup auth token in session
            var tokenBytes = Encoding.UTF8.GetBytes("valid-token");
            _sessionMock.Setup(s => s.TryGetValue("AuthToken", out It.Ref<byte[]>.IsAny!))
                .Callback(new TryGetValueCallback((string key, out byte[] value) => {
                    value = tokenBytes;
                }))
                .Returns(true);

            // Setup mock customer response - succeeds
            var customers = new List<CustomerDto>
            {
                new() { Id = Guid.NewGuid(), Name = "Customer 1" },
                new() { Id = Guid.NewGuid(), Name = "Customer 2" }
            };

            // Setup HTTP responses - customer succeeds, product fails
            SetupHttpResponse("/customers", HttpStatusCode.OK, JsonSerializer.Serialize(customers));
            SetupHttpResponse("/products", HttpStatusCode.InternalServerError, "Error");

            // Setup OrderService mock to return orders
            var orders = new List<OrderDto>
            {
                new() { Id = Guid.NewGuid(), CustomerId = Guid.NewGuid(), TotalPrice = 25.75m },
                new() { Id = Guid.NewGuid(), CustomerId = Guid.NewGuid(), TotalPrice = 32.50m }
            };
            _orderServiceMock.Setup(x => x.GetAllOrdersAsync())
                .ReturnsAsync(orders);

            // Act
            var result = await _dashboardService.GetDashboardStatsAsync();

            // Assert
            result.Should().NotBeNull();
            result.CustomerCount.Should().Be(2);    // Success
            result.ProductCount.Should().Be(0);     // Failed API call
            result.OrderCount.Should().Be(2);       // Success
            result.TotalSales.Should().Be(58.25m);  // Success
        }

        [Fact]
        public async Task GetDashboardStatsAsync_HandlesNoAuthToken()
        {
            // Arrange
            // Don't set up auth token in session

            // Setup OrderService mock to return empty list (no auth token)
            _orderServiceMock.Setup(x => x.GetAllOrdersAsync())
                .ReturnsAsync(new List<OrderDto>());

            // Act
            var result = await _dashboardService.GetDashboardStatsAsync();

            // Assert
            result.Should().NotBeNull();
            result.CustomerCount.Should().Be(0);
            result.ProductCount.Should().Be(0);
            result.OrderCount.Should().Be(0);
            result.TotalSales.Should().Be(0);

            // Verify no authorization header was sent
            _httpMessageHandlerMock.Protected()
                .Verify<Task<HttpResponseMessage>>(
                    "SendAsync",
                    Times.AtLeastOnce(),
                    ItExpr.Is<HttpRequestMessage>(req =>
                        !req.Headers.Contains("Authorization")),
                    ItExpr.IsAny<CancellationToken>());
        }

        private void SetupHttpResponse(string requestUri, HttpStatusCode statusCode, string content)
        {
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.RequestUri!.PathAndQuery == requestUri),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(content)
                });
        }

        private void VerifyAuthorizationHeader(string expectedAuthHeader)
        {
            _httpMessageHandlerMock.Protected()
                .Verify<Task<HttpResponseMessage>>(
                    "SendAsync",
                    Times.AtLeastOnce(),
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Headers.Authorization != null &&
                        req.Headers.Authorization.ToString() == expectedAuthHeader),
                    ItExpr.IsAny<CancellationToken>());
        }
    }

    // Dashboard stats class if not already defined in the test project
    public class DashboardStats
    {
        public int CustomerCount { get; set; }
        public int ProductCount { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalSales { get; set; }
    }
}
