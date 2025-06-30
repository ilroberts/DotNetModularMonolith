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
        private readonly Mock<ICustomerService> _customerServiceMock; // Changed to interface
        private readonly Mock<IProductService> _productServiceMock;   // Changed to interface

        public OrderServiceTests()
        {
            _loggerMock = new Mock<ILogger<OrderService>>();
            _customerServiceMock = new Mock<ICustomerService>(); // Use interface mock
            _productServiceMock = new Mock<IProductService>();   // Use interface mock

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
            // Create API format orders - this is what the service expects from the API
            var apiOrders = new List<ApiOrderDto>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    CustomerId = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    Items =
                    [
                        new ApiOrderItemDto { ProductId = Guid.NewGuid(), Quantity = 1, Price = 29.99m }
                    ]
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    CustomerId = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    Items =
                    [
                        new ApiOrderItemDto { ProductId = Guid.NewGuid(), Quantity = 2, Price = 19.99m }
                    ]
                }
            };
            var jsonResponse = JsonSerializer.Serialize(apiOrders);

            SetupAuthToken("valid-token");
            SetupUsername("test-user");

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            };

            SetupAuthServiceExecuteWithTokenRefresh(true, httpResponse);

            // Set up mock responses for customer and product service calls
            // These are needed for the EnrichOrderWithDetailsAsync method
            _customerServiceMock.Setup(x => x.GetCustomerByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new CustomerDto { Id = Guid.NewGuid(), Name = "Test Customer" });

            _productServiceMock.Setup(x => x.GetProductByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new ProductDto { Id = Guid.NewGuid(), Name = "Test Product" });

            // Act
            var result = await _orderService.GetAllOrdersAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Status.Should().Be("Completed"); // Default status set by OrderService
            result[1].Status.Should().Be("Completed"); // Default status set by OrderService

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
            var productId = Guid.NewGuid();

            // Use ApiOrderDto format which is what the service expects to deserialize
            var apiOrder = new ApiOrderDto
            {
                Id = orderId,
                CustomerId = customerId,
                CreatedAt = DateTime.UtcNow,
                Items =
                [
                    new ApiOrderItemDto { ProductId = productId, Quantity = 2, Price = 19.99m }
                ]
            };

            var jsonResponse = JsonSerializer.Serialize(apiOrder);

            SetupAuthToken("valid-token");
            SetupUsername("test-user");

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            };

            SetupAuthServiceExecuteWithTokenRefresh(true, httpResponse);

            // Set up mock responses for customer and product services
            _customerServiceMock.Setup(x => x.GetCustomerByIdAsync(customerId))
                .ReturnsAsync(new CustomerDto { Id = customerId, Name = "Test Customer" });

            _productServiceMock.Setup(x => x.GetProductByIdAsync(productId))
                .ReturnsAsync(new ProductDto { Id = productId, Name = "Test Product" });

            // Act
            var result = await _orderService.GetOrderByIdAsync(orderId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(orderId);
            result.CustomerId.Should().Be(customerId);
            result.ProductId.Should().Be(productId);
            result.TotalPrice.Should().Be(19.99m * 2);
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
