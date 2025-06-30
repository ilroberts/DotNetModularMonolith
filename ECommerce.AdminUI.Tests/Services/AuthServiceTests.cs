using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;
using ECommerce.AdminUI.Services;

namespace ECommerce.AdminUI.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<ILogger<AuthService>> _mockLogger;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockLogger = new Mock<ILogger<AuthService>>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://test.api.com")
        };

        _mockHttpClientFactory.Setup(x => x.CreateClient("TokenService"))
            .Returns(_httpClient);

        _authService = new AuthService(_mockHttpClientFactory.Object, _mockLogger.Object);
    }

    #region GenerateTokenAsync Tests

    [Fact]
    public async Task GenerateTokenAsync_WithValidUser_ReturnsToken()
    {
        // Arrange
        const string userName = "testuser";
        const string expectedToken = "test-token-123";

        SetupHttpResponse(HttpStatusCode.OK, expectedToken);

        // Act
        var result = await _authService.GenerateTokenAsync(userName);

        // Assert
        Assert.Equal(expectedToken, result);
        VerifyHttpCall(HttpMethod.Post, "/modulith/admin/generateToken");
        VerifyLogInformation("Generating token for user {UserName}, request body: {RequestBody}");
        VerifyLogInformation("Response status: {Status}, Content: {Content}");
    }

    [Fact]
    public async Task GenerateTokenAsync_WithValidUser_TrimsWhitespace()
    {
        // Arrange
        const string userName = "testuser";
        const string tokenWithWhitespace = "  test-token-123  ";
        const string expectedToken = "test-token-123";

        SetupHttpResponse(HttpStatusCode.OK, tokenWithWhitespace);

        // Act
        var result = await _authService.GenerateTokenAsync(userName);

        // Assert
        Assert.Equal(expectedToken, result);
    }

    [Fact]
    public async Task GenerateTokenAsync_WithEmptyResponse_ReturnsNull()
    {
        // Arrange
        const string userName = "testuser";

        SetupHttpResponse(HttpStatusCode.OK, "");

        // Act
        var result = await _authService.GenerateTokenAsync(userName);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GenerateTokenAsync_WithWhitespaceResponse_ReturnsNull()
    {
        // Arrange
        const string userName = "testuser";

        SetupHttpResponse(HttpStatusCode.OK, "   ");

        // Act
        var result = await _authService.GenerateTokenAsync(userName);

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task GenerateTokenAsync_WithErrorStatusCode_ReturnsNull(HttpStatusCode statusCode)
    {
        // Arrange
        const string userName = "testuser";

        SetupHttpResponse(statusCode, "error response");

        // Act
        var result = await _authService.GenerateTokenAsync(userName);

        // Assert
        Assert.Null(result);
        VerifyLogError("Token generation failed with status code {StatusCode}");
    }

    [Fact]
    public async Task GenerateTokenAsync_WithHttpException_ReturnsNull()
    {
        // Arrange
        const string userName = "testuser";
        var expectedException = new HttpRequestException("Network error");

        SetupHttpException(expectedException);

        // Act
        var result = await _authService.GenerateTokenAsync(userName);

        // Assert
        Assert.Null(result);
        VerifyLogError("Error generating token for user {UserName}");
    }

    #endregion

    #region RefreshTokenAsync Tests

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ReturnsNewToken()
    {
        // Arrange
        const string token = "old-token";
        const string expectedNewToken = "new-token-456";

        SetupHttpResponse(HttpStatusCode.OK, expectedNewToken);

        // Act
        var result = await _authService.RefreshTokenAsync(token);

        // Assert
        Assert.Equal(expectedNewToken, result);
        VerifyHttpCall(HttpMethod.Post, "/modulith/admin/refreshToken", token);
        VerifyLogDebug("Attempting to refresh token");
        VerifyLogInformation("Token refreshed successfully");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_TrimsWhitespace()
    {
        // Arrange
        const string token = "old-token";
        const string tokenWithWhitespace = "  new-token-456  ";
        const string expectedToken = "new-token-456";

        SetupHttpResponse(HttpStatusCode.OK, tokenWithWhitespace);

        // Act
        var result = await _authService.RefreshTokenAsync(token);

        // Assert
        Assert.Equal(expectedToken, result);
    }

    [Fact]
    public async Task RefreshTokenAsync_WithEmptyResponse_ReturnsNull()
    {
        // Arrange
        const string token = "old-token";

        SetupHttpResponse(HttpStatusCode.OK, "");

        // Act
        var result = await _authService.RefreshTokenAsync(token);

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task RefreshTokenAsync_WithErrorStatusCode_ReturnsNull(HttpStatusCode statusCode)
    {
        // Arrange
        const string token = "old-token";

        SetupHttpResponse(statusCode, "error response");

        // Act
        var result = await _authService.RefreshTokenAsync(token);

        // Assert
        Assert.Null(result);
        VerifyLogWarning("Token refresh failed with status code {StatusCode}");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithException_ReturnsNull()
    {
        // Arrange
        const string token = "old-token";
        var expectedException = new HttpRequestException("Network error");

        SetupHttpException(expectedException);

        // Act
        var result = await _authService.RefreshTokenAsync(token);

        // Assert
        Assert.Null(result);
        VerifyLogError("Error refreshing token");
    }

    #endregion

    #region ExecuteWithTokenRefreshAsync Tests

    [Fact]
    public async Task ExecuteWithTokenRefreshAsync_WithSuccessfulFirstCall_ReturnsSuccessAndResponse()
    {
        // Arrange
        const string token = "valid-token";
        const string username = "testuser";
        var httpContext = CreateMockHttpContext();
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

        var apiCall = new Mock<Func<string, Task<HttpResponseMessage>>>();
        apiCall.Setup(x => x(token)).ReturnsAsync(expectedResponse);

        // Act
        var result = await _authService.ExecuteWithTokenRefreshAsync(
            apiCall.Object, token, username, httpContext.Object);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(expectedResponse, result.Response);
        apiCall.Verify(x => x(token), Times.Once);
    }

    [Fact]
    public async Task ExecuteWithTokenRefreshAsync_WithUnauthorizedThenSuccessfulRefresh_ReturnsSuccessAndResponse()
    {
        // Arrange
        const string oldToken = "expired-token";
        const string newToken = "refreshed-token";
        const string username = "testuser";

        // Set up the session mock to properly capture interactions
        var mockSession = new Mock<ISession>();
        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(x => x.Session).Returns(mockSession.Object);

        var unauthorizedResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized);
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        var apiCall = new Mock<Func<string, Task<HttpResponseMessage>>>();
        apiCall.SetupSequence(x => x(It.IsAny<string>()))
            .ReturnsAsync(unauthorizedResponse)
            .ReturnsAsync(successResponse);

        // Setup refresh token response
        SetupHttpResponse(HttpStatusCode.OK, newToken);

        // Act
        var result = await _authService.ExecuteWithTokenRefreshAsync(
            apiCall.Object, oldToken, username, mockHttpContext.Object);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(successResponse, result.Response);
        apiCall.Verify(x => x(oldToken), Times.Once);
        apiCall.Verify(x => x(newToken), Times.Once);
        VerifyLogInformation("Received 401 Unauthorized, attempting to refresh token");

        // Verify session was updated with new token
        // Use Set method directly instead of the extension method SetString
        mockSession.Verify(s => s.Set(
            It.Is<string>(key => key == "AuthToken"),
            It.Is<byte[]>(val => Encoding.UTF8.GetString(val).Contains(newToken))),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteWithTokenRefreshAsync_WithUnauthorizedAndFailedRefresh_ClearsSessionAndReturnsFalse()
    {
        // Arrange
        const string token = "expired-token";
        const string username = "testuser";
        var httpContext = CreateMockHttpContext();

        var unauthorizedResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized);

        var apiCall = new Mock<Func<string, Task<HttpResponseMessage>>>();
        apiCall.Setup(x => x(token)).ReturnsAsync(unauthorizedResponse);

        // Setup failed refresh token response
        SetupHttpResponse(HttpStatusCode.Unauthorized, "");

        // Act
        var result = await _authService.ExecuteWithTokenRefreshAsync(
            apiCall.Object, token, username, httpContext.Object);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Response);
        VerifyLogWarning("Token refresh failed, user needs to re-authenticate");

        // Verify session was cleared
        httpContext.Verify(x => x.Session.Remove("AuthToken"), Times.Once);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.NotFound)]
    public async Task ExecuteWithTokenRefreshAsync_WithNonUnauthorizedError_ReturnsFailureAndResponse(HttpStatusCode statusCode)
    {
        // Arrange
        const string token = "valid-token";
        const string username = "testuser";
        var httpContext = CreateMockHttpContext();
        var errorResponse = new HttpResponseMessage(statusCode);

        var apiCall = new Mock<Func<string, Task<HttpResponseMessage>>>();
        apiCall.Setup(x => x(token)).ReturnsAsync(errorResponse);

        // Act
        var result = await _authService.ExecuteWithTokenRefreshAsync(
            apiCall.Object, token, username, httpContext.Object);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(errorResponse, result.Response);
        apiCall.Verify(x => x(token), Times.Once);
    }

    [Fact]
    public async Task ExecuteWithTokenRefreshAsync_WithException_ReturnsFailureAndNull()
    {
        // Arrange
        const string token = "valid-token";
        const string username = "testuser";
        var httpContext = CreateMockHttpContext();
        var expectedException = new InvalidOperationException("Test exception");

        var apiCall = new Mock<Func<string, Task<HttpResponseMessage>>>();
        apiCall.Setup(x => x(token)).ThrowsAsync(expectedException);

        // Act
        var result = await _authService.ExecuteWithTokenRefreshAsync(
            apiCall.Object, token, username, httpContext.Object);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Response);
        VerifyLogError("Error executing authenticated request");
    }

    #endregion

    #region Helper Methods

    private void SetupHttpResponse(HttpStatusCode statusCode, string content)
    {
        var response = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(content, Encoding.UTF8, "text/plain")
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
    }

    private void SetupHttpException(Exception exception)
    {
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(exception);
    }

    private Mock<HttpContext> CreateMockHttpContext()
    {
        var mockSession = new Mock<ISession>();
        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(x => x.Session).Returns(mockSession.Object);
        return mockHttpContext;
    }

    private void VerifyHttpCall(HttpMethod method, string expectedUri, string? expectedToken = null)
    {
        _mockHttpMessageHandler.Protected()
            .Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == method &&
                    req.RequestUri != null &&
                    req.RequestUri.ToString().EndsWith(expectedUri) &&
                    (expectedToken == null || req.Headers.Authorization != null &&
                     req.Headers.Authorization.ToString() == $"Bearer {expectedToken}")),
                ItExpr.IsAny<CancellationToken>());
    }

    private void VerifyLogInformation(string expectedMessage)
    {
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    private void VerifyLogDebug(string expectedMessage)
    {
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    private void VerifyLogWarning(string expectedMessage)
    {
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    private void VerifyLogError(string expectedMessage)
    {
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion
}
