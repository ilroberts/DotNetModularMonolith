using ECommerce.AdminUI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;

namespace ECommerce.AdminUI.Tests.Services
{
    public abstract class BaseServiceTests
    {
        protected Mock<IHttpContextAccessor> HttpContextAccessorMock { get; }
        protected Mock<IHttpClientFactory> HttpClientFactoryMock { get; }
        protected Mock<HttpMessageHandler> HttpMessageHandlerMock { get; }
        protected Mock<AuthService> AuthServiceMock { get; }
        protected HttpClient HttpClient { get; }

        protected BaseServiceTests()
        {
            // Setup HTTP context accessor with session
            var httpContext = new DefaultHttpContext();
            var sessionMock = new Mock<ISession>();
            var sessionDict = new Dictionary<string, string>();

            sessionMock.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
                .Callback<string, byte[]>((key, value) =>
                {
                    var stringValue = System.Text.Encoding.UTF8.GetString(value);
                    sessionDict[key] = stringValue;
                });

            sessionMock.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) =>
                {
                    if (sessionDict.TryGetValue(key, out var stringValue))
                    {
                        value = System.Text.Encoding.UTF8.GetBytes(stringValue);
                        return true;
                    }
                    value = null;
                    return false;
                });

            httpContext.Session = sessionMock.Object;

            HttpContextAccessorMock = new Mock<IHttpContextAccessor>();
            HttpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

            // Setup HTTP client factory and handler
            HttpMessageHandlerMock = new Mock<HttpMessageHandler>();
            HttpClient = new HttpClient(HttpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("http://test-api.com/")
            };

            HttpClientFactoryMock = new Mock<IHttpClientFactory>();
            HttpClientFactoryMock
                .Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(HttpClient);

            // Setup auth service mock
            AuthServiceMock = new Mock<AuthService>(
                HttpClientFactoryMock.Object,
                Mock.Of<ILogger<AuthService>>(),
                HttpContextAccessorMock.Object);
        }

        protected void SetupAuthToken(string token = "test-token")
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(token);
            HttpContextAccessorMock.Object.HttpContext!.Session.Set("AuthToken", bytes);
        }

        protected void SetupUsername(string username = "testuser")
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(username);
            HttpContextAccessorMock.Object.HttpContext!.Session.Set("Username", bytes);
        }

        protected void SetupHttpResponse(HttpStatusCode statusCode, string content)
        {
            HttpMessageHandlerMock
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

        protected void SetupAuthServiceExecuteWithTokenRefresh(bool success, HttpResponseMessage? response = null)
        {
            AuthServiceMock
                .Setup(x => x.ExecuteWithTokenRefreshAsync(
                    It.IsAny<Func<string, Task<HttpResponseMessage>>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<HttpContext>()))
                .ReturnsAsync((success, response));
        }
    }
}
