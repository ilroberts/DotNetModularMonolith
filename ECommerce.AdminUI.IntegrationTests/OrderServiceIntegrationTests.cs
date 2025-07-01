using System;
using System.Net;
using System.Threading.Tasks;
using ECommerce.AdminUI.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace ECommerce.AdminUI.IntegrationTests;

public class OrderServiceIntegrationTests : ServiceIntegrationTestBase
{
    public OrderServiceIntegrationTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<IOrderService, OrderService>();
    }

    [Fact]
    public async Task GetOrdersAsync_ReturnsStubbedOrders()
    {
        // Arrange
        var httpClient = CreateMockHttpClient(HttpStatusCode.OK, "[]");
        var services = SetupCommonServices(httpClient);
        var provider = services.BuildServiceProvider();
        var orderService = provider.GetRequiredService<IOrderService>();

        // Act
        var orders = await orderService.GetAllOrdersAsync();

        // Assert
        Assert.NotNull(orders);
        Assert.Empty(orders);
    }

    [Fact]
    public async Task CreateOrderAsync_ReturnsTrue_WhenApiReturnsSuccess()
    {
        // Arrange
        var httpClient = CreateMockHttpClient(HttpStatusCode.OK, "true");
        var services = SetupCommonServices(httpClient, setupAuthSuccess: true);
        var provider = services.BuildServiceProvider();
        var orderService = provider.GetRequiredService<IOrderService>();

        var order = new OrderDto
        {
            Id = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            CustomerName = "Test Customer",
            CustomerEmail = "test@example.com",
            ProductId = Guid.NewGuid(),
            ProductName = "Test Product",
            ProductPrice = 10.0m,
            Quantity = 2,
            TotalPrice = 20.0m,
            CreatedAt = DateTime.UtcNow,
            Status = "Created",
            OrderDate = DateTime.UtcNow,
            OrderStatus = "Pending"
        };

        // Act
        var result = await orderService.CreateOrderAsync(order);

        // Log the result for debugging
        if (!result)
        {
            var requestJson = System.Text.Json.JsonSerializer.Serialize(order);
            TestOutputHelper.WriteLine($"Order payload: {requestJson}");
            TestOutputHelper.WriteLine("Stubbed HTTP response: 200 OK, body: 'true'");
        }

        // Assert
        Assert.True(result);
    }
}
