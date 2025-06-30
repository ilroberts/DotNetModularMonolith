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
    public class ProductServiceTests : BaseServiceTests
    {
        private readonly ProductService _productService;
        private readonly Mock<ILogger<ProductService>> _loggerMock;

        public ProductServiceTests()
        {
            _loggerMock = new Mock<ILogger<ProductService>>();
            _productService = new ProductService(
                HttpClientFactoryMock.Object,
                _loggerMock.Object,
                HttpContextAccessorMock.Object,
                AuthServiceMock.Object
            );
        }

        [Fact]
        public async Task GetAllProductsAsync_WithValidToken_ReturnsProducts()
        {
            // Arrange
            var products = new List<ProductDto>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Test Product 1",
                    Description = "Description 1",
                    Price = 19.99m
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Test Product 2",
                    Description = "Description 2",
                    Price = 29.99m
                }
            };
            var jsonResponse = JsonSerializer.Serialize(products);

            SetupAuthToken("valid-token");
            SetupUsername("test-user");

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            };

            SetupAuthServiceExecuteWithTokenRefresh(true, httpResponse);

            // Act
            var result = await _productService.GetAllProductsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Name.Should().Be("Test Product 1");
            result[1].Name.Should().Be("Test Product 2");

            // Verify AuthService was called with correct parameters
            AuthServiceMock.Verify(x => x.ExecuteWithTokenRefreshAsync(
                It.IsAny<Func<string, Task<HttpResponseMessage>>>(),
                "valid-token",
                "test-user",
                HttpContextAccessorMock.Object.HttpContext!),
                Times.Once);
        }

        [Fact]
        public async Task GetAllProductsAsync_WithNoToken_ReturnsEmptyList()
        {
            // Arrange
            // Don't set up a token in the session

            // Act
            var result = await _productService.GetAllProductsAsync();

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
        public async Task GetProductByIdAsync_WithValidToken_ReturnsProduct()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var product = new ProductDto
            {
                Id = productId,
                Name = "Test Product",
                Description = "Test Description",
                Price = 19.99m
            };
            var jsonResponse = JsonSerializer.Serialize(product);

            SetupAuthToken("valid-token");
            SetupUsername("test-user");

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            };

            SetupAuthServiceExecuteWithTokenRefresh(true, httpResponse);

            // Act
            var result = await _productService.GetProductByIdAsync(productId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(productId);
            result.Name.Should().Be("Test Product");
            result.Price.Should().Be(19.99m);
        }

        [Fact]
        public async Task GetProductByIdAsync_WithNoToken_ReturnsNull()
        {
            // Arrange
            var productId = Guid.NewGuid();
            // Don't set up a token in the session

            // Act
            var result = await _productService.GetProductByIdAsync(productId);

            // Assert
            result.Should().BeNull();
        }
    }
}
