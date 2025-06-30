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
    public class CustomerServiceTests : BaseServiceTests
    {
        private readonly CustomerService _customerService;
        private readonly Mock<ILogger<CustomerService>> _loggerMock;

        public CustomerServiceTests()
        {
            _loggerMock = new Mock<ILogger<CustomerService>>();
            _customerService = new CustomerService(
                HttpClientFactoryMock.Object,
                _loggerMock.Object,
                HttpContextAccessorMock.Object,
                AuthServiceMock.Object
            );
        }

        [Fact]
        public async Task GetAllCustomersAsync_WithValidToken_ReturnsCustomers()
        {
            // Arrange
            var customers = new List<CustomerDto>
            {
                new() { Id = Guid.NewGuid(), Name = "Test Customer 1", Email = "test1@example.com" },
                new() { Id = Guid.NewGuid(), Name = "Test Customer 2", Email = "test2@example.com" }
            };
            var jsonResponse = JsonSerializer.Serialize(customers);

            SetupAuthToken("valid-token");
            SetupUsername("test-user");

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            };

            SetupAuthServiceExecuteWithTokenRefresh(true, httpResponse);

            // Act
            var result = await _customerService.GetAllCustomersAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Name.Should().Be("Test Customer 1");
            result[1].Name.Should().Be("Test Customer 2");

            // Verify AuthService was called with correct parameters
            AuthServiceMock.Verify(x => x.ExecuteWithTokenRefreshAsync(
                It.IsAny<Func<string, Task<HttpResponseMessage>>>(),
                "valid-token",
                "test-user",
                HttpContextAccessorMock.Object.HttpContext!),
                Times.Once);
        }

        [Fact]
        public async Task GetAllCustomersAsync_WithNoToken_ReturnsEmptyList()
        {
            // Arrange
            // Don't set up a token in the session

            // Act
            var result = await _customerService.GetAllCustomersAsync();

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
        public async Task GetAllCustomersAsync_WhenAuthServiceFails_ReturnsEmptyList()
        {
            // Arrange
            SetupAuthToken("invalid-token");
            SetupUsername("test-user");
            SetupAuthServiceExecuteWithTokenRefresh(false, null);

            // Act
            var result = await _customerService.GetAllCustomersAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetCustomerByIdAsync_WithValidToken_ReturnsCustomer()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var customer = new CustomerDto
            {
                Id = customerId,
                Name = "Test Customer",
                Email = "test@example.com"
            };
            var jsonResponse = JsonSerializer.Serialize(customer);

            SetupAuthToken("valid-token");
            SetupUsername("test-user");

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            };

            SetupAuthServiceExecuteWithTokenRefresh(true, httpResponse);

            // Act
            var result = await _customerService.GetCustomerByIdAsync(customerId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(customerId);
            result.Name.Should().Be("Test Customer");
            result.Email.Should().Be("test@example.com");

            // Verify AuthService was called with correct parameters
            AuthServiceMock.Verify(x => x.ExecuteWithTokenRefreshAsync(
                It.IsAny<Func<string, Task<HttpResponseMessage>>>(),
                "valid-token",
                "test-user",
                HttpContextAccessorMock.Object.HttpContext!),
                Times.Once);
        }

        [Fact]
        public async Task GetCustomerByIdAsync_WithNoToken_ReturnsNull()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            // Don't set up a token in the session

            // Act
            var result = await _customerService.GetCustomerByIdAsync(customerId);

            // Assert
            result.Should().BeNull();

            // Verify AuthService was not called
            AuthServiceMock.Verify(x => x.ExecuteWithTokenRefreshAsync(
                It.IsAny<Func<string, Task<HttpResponseMessage>>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<HttpContext>()),
                Times.Never);
        }

        [Fact]
        public async Task GetCustomerByIdAsync_WhenAuthServiceFails_ReturnsNull()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            SetupAuthToken("invalid-token");
            SetupUsername("test-user");
            SetupAuthServiceExecuteWithTokenRefresh(false, null);

            // Act
            var result = await _customerService.GetCustomerByIdAsync(customerId);

            // Assert
            result.Should().BeNull();
        }
    }
}
