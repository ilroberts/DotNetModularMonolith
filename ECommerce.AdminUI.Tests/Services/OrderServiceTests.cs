using ECommerce.AdminUI.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Text.Json;
using Xunit;

namespace ECommerce.AdminUI.Tests.Services
{
    public class OrderServiceTests : BaseServiceTests
    {
        private readonly OrderService _orderService;
        private readonly Mock<ILogger<OrderService>> _loggerMock;
        private readonly Mock<CustomerService> _customerServiceMock;
        private readonly Mock<ProductService> _productServiceMock;

        public OrderServiceTests()
        {
            _loggerMock = new Mock<ILogger<OrderService>>();
            _customerServiceMock = new Mock<CustomerService>(
                HttpClientFactoryMock.Object,
                Mock.Of<ILogger<CustomerService>>(),
                HttpContextAccessorMock.Object,
                AuthServiceMock.Object
            );
            _productServiceMock = new Mock<ProductService>(
                HttpClientFactoryMock.Object,
                Mock.Of<ILogger<ProductService>>(),
                HttpContextAccessorMock.Object,
                AuthServiceMock.Object
            );

            _orderService = new OrderService(
                HttpClientFactoryMock.Object,
                _loggerMock.Object,
                HttpContextAccessorMock.Object,
                _customerServiceMock.Object,
                _productServiceMock.Object,
                AuthServiceMock.Object
            );
        }

        [Fact]
        public async Task GetAllOrdersAsync_WithValidToken_ReturnsOrders()
        {
            // Arrange
            var orders = new List<OrderDto>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    CustomerId = Guid.NewGuid(),
                    OrderDate = DateTime.UtcNow,
                    OrderStatus = "Pending"
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    CustomerId = Guid.NewGuid(),
                    OrderDate = DateTime.UtcNow,
                    OrderStatus = "Shipped"
                }
            };
            var jsonResponse = JsonSerializer.Serialize(orders);

            SetupAuthToken("valid-token");
            SetupUsername("test-user");

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            };

            SetupAuthServiceExecuteWithTokenRefresh(true, httpResponse);

            // Act
            var result = await _orderService.GetAllOrdersAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].OrderStatus.Should().Be("Pending");
            result[1].OrderStatus.Should().Be("Shipped");

            // Verify AuthService was called with correct parameters
            AuthServiceMock.Verify(x => x.ExecuteWithTokenRefreshAsync(
                It.IsAny<Func<string, Task<HttpResponseMessage>>>(),
                "valid-token",
                "test-user",
                HttpContextAccessorMock.Object.HttpContext!),
                Times.Once);
        }

        [Fact]
        public async Task GetAllOrdersAsync_WithNoToken_ReturnsEmptyList()
        {
            // Arrange
            // Don't set up a token in the session

            // Act
            var result = await _orderService.GetAllOrdersAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            // Verify AuthService was not called
            AuthServiceMock.Verify(x => x.ExecuteWithTokenRefreshAsync(
                It.IsAny<Func<string, Task<HttpResponseMessage>>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<HttpContext>()),
                Times.Never);
        }

        [Fact]
        public async Task GetOrderByIdAsync_WithValidToken_ReturnsOrder()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var order = new OrderDto
            {
                Id = orderId,
                CustomerId = customerId,
                OrderDate = DateTime.UtcNow,
                OrderStatus = "Processing"
            };
            var jsonResponse = JsonSerializer.Serialize(order);

            SetupAuthToken("valid-token");
            SetupUsername("test-user");

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            };

            SetupAuthServiceExecuteWithTokenRefresh(true, httpResponse);

            // Act
            var result = await _orderService.GetOrderByIdAsync(orderId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(orderId);
            result.CustomerId.Should().Be(customerId);
            result.OrderStatus.Should().Be("Processing");
        }

        [Fact]
        public async Task GetOrderByIdAsync_WithNoToken_ReturnsNull()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            // Don't set up a token in the session

            // Act
            var result = await _orderService.GetOrderByIdAsync(orderId);

            // Assert
            result.Should().BeNull();
        }
    }
}
