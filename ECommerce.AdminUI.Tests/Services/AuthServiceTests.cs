using ECommerce.AdminUI.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using Xunit;

namespace ECommerce.AdminUI.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<ILogger<AuthService>> _loggerMock;
        private readonly Mock<IConfiguration> _configMock;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;
        private readonly AuthService _authService;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly DefaultHttpContext _httpContext;
        private readonly Mock<ISession> _sessionMock;

        public AuthServiceTests()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("http://test-token-service.com/")
            };

            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _httpClientFactoryMock
                .Setup(x => x.CreateClient("TokenService"))
                .Returns(_httpClient);

            _loggerMock = new Mock<ILogger<AuthService>>();
            _configMock = new Mock<IConfiguration>();

            // Set up the session mock
            _sessionMock = new Mock<ISession>();
            var sessionDict = new Dictionary<string, string>();

            _sessionMock.Setup(s => s.SetString(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((key, value) => sessionDict[key] = value);

            _sessionMock.Setup(s => s.GetString(It.IsAny<string>()))
                .Returns<string>(key => sessionDict.TryGetValue(key, out var value) ? value : null);

            _sessionMock.Setup(s => s.Remove(It.IsAny<string>()))
                .Callback<string>(key => sessionDict.Remove(key));

            // Set up HTTP context with session
            _httpContext = new DefaultHttpContext
            {
                Session = _sessionMock.Object
            };

            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(_httpContext);

            _authService = new AuthService(_httpClientFactoryMock.Object, _loggerMock.Object, _configMock.Object);
        }

        [Fact]
        public async Task GenerateTokenAsync_ReturnsToken_WhenSuccessful()
        {
            // Arrange
            var expectedToken = "valid-token-123";
            SetupHttpResponse(HttpStatusCode.OK, expectedToken);

            // Act
            var result = await _authService.GenerateTokenAsync("testuser");

            // Assert
            result.Should().Be(expectedToken);
            VerifyHttpRequest(HttpMethod.Post, "/modulith/admin/generateToken");
        }

        [Fact]
        public async Task GenerateTokenAsync_ReturnsNull_WhenApiCallFails()
        {
            // Arrange
            SetupHttpResponse(HttpStatusCode.BadRequest, "Error");

            // Act
            var result = await _authService.GenerateTokenAsync("testuser");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task RefreshTokenAsync_ReturnsNewToken_WhenSuccessful()
        {
            // Arrange
            var newToken = "refreshed-token-456";
            SetupHttpResponse(HttpStatusCode.OK, newToken);

            // Act
            var result = await _authService.RefreshTokenAsync("old-token");

            // Assert
            result.Should().Be(newToken);

            // Verify the request was made with the correct authorization header
            _httpMessageHandlerMock.Protected()
                .Verify<Task<HttpResponseMessage>>(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri!.PathAndQuery == "/modulith/admin/refreshToken" &&
                        req.Headers.Authorization!.Parameter == "old-token"),
                    ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task RefreshTokenAsync_ReturnsNull_WhenApiCallFails()
        {
            // Arrange
            SetupHttpResponse(HttpStatusCode.BadRequest, "Error");

            // Act
            var result = await _authService.RefreshTokenAsync("invalid-token");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task ExecuteWithTokenRefreshAsync_ReturnsSuccessResponse_WhenInitialCallSucceeds()
        {
            // Arrange
            var successResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Success")
            };

            bool apiCallInvoked = false;
            Func<string, Task<HttpResponseMessage>> apiCall = token =>
            {
                apiCallInvoked = true;
                token.Should().Be("test-token");
                return Task.FromResult(successResponse);
            };

            // Act
            var result = await _authService.ExecuteWithTokenRefreshAsync(
                apiCall,
                "test-token",
                "testuser",
                _httpContext);

            // Assert
            apiCallInvoked.Should().BeTrue();
            result.Success.Should().BeTrue();
            result.Response.Should().BeSameAs(successResponse);
        }

        [Fact]
        public async Task ExecuteWithTokenRefreshAsync_AttemptsTokenRefresh_WhenInitialCallReturns401()
        {
            // Arrange
            // Setup initial 401 response
            var unauthorizedResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized);

            // Setup successful response after token refresh
            var successResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Success after refresh")
            };

            int apiCallCount = 0;
            Func<string, Task<HttpResponseMessage>> apiCall = token =>
            {
                apiCallCount++;
                if (apiCallCount == 1)
                {
                    token.Should().Be("old-token");
                    return Task.FromResult(unauthorizedResponse);
                }
                else
                {
                    token.Should().Be("new-token");
                    return Task.FromResult(successResponse);
                }
            };

            // Setup refresh token response
            SetupHttpResponse(HttpStatusCode.OK, "new-token");

            // Act
            var result = await _authService.ExecuteWithTokenRefreshAsync(
                apiCall,
                "old-token",
                "testuser",
                _httpContext);

            // Assert
            apiCallCount.Should().Be(2);
            result.Success.Should().BeTrue();
            result.Response.Should().BeSameAs(successResponse);

            // Verify token was updated in session
            _sessionMock.Verify(s => s.SetString("AuthToken", "new-token"), Times.Once);
        }

        [Fact]
        public async Task ExecuteWithTokenRefreshAsync_ReturnsFailure_WhenRefreshFails()
        {
            // Arrange
            // Setup initial 401 response
            var unauthorizedResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized);

            int apiCallCount = 0;
            Func<string, Task<HttpResponseMessage>> apiCall = token =>
            {
                apiCallCount++;
                return Task.FromResult(unauthorizedResponse);
            };

            // Setup failed token refresh
            SetupHttpResponse(HttpStatusCode.BadRequest, "Error");

            // Act
            var result = await _authService.ExecuteWithTokenRefreshAsync(
                apiCall,
                "invalid-token",
                "testuser",
                _httpContext);

            // Assert
            apiCallCount.Should().Be(1);
            result.Success.Should().BeFalse();
            result.Response.Should().BeNull();

            // Verify token was removed from session
            _sessionMock.Verify(s => s.Remove("AuthToken"), Times.Once);
        }

        private void SetupHttpResponse(HttpStatusCode statusCode, string content)
        {
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(content)
                });
        }

        private void VerifyHttpRequest(HttpMethod method, string requestUri)
        {
            _httpMessageHandlerMock
                .Protected()
                .Verify<Task<HttpResponseMessage>>(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == method &&
                        req.RequestUri!.PathAndQuery == requestUri),
                    ItExpr.IsAny<CancellationToken>());
        }
    }
}
