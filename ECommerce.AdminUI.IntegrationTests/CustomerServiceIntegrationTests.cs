using System;
using System.Net;
using System.Threading.Tasks;
using ECommerce.AdminUI.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace ECommerce.AdminUI.IntegrationTests;

public class CustomerServiceIntegrationTests : ServiceIntegrationTestBase
{
    public CustomerServiceIntegrationTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<ICustomerService, CustomerService>();
    }

    [Fact]
    public async Task GetAllCustomersAsync_ReturnsStubbedCustomers()
    {
        // Arrange
        var customersJson = "[]";
        var httpClient = CreateMockHttpClient(HttpStatusCode.OK, customersJson);
        var services = SetupCommonServices(httpClient);
        var provider = services.BuildServiceProvider();
        var customerService = provider.GetRequiredService<ICustomerService>();

        // Act
        var customers = await customerService.GetAllCustomersAsync();

        // Assert
        Assert.NotNull(customers);
        Assert.Empty(customers);
    }

    [Fact]
    public async Task CreateCustomerAsync_ReturnsTrue_WhenApiReturnsSuccess()
    {
        // Arrange
        var httpClient = CreateMockHttpClient(HttpStatusCode.OK, "true");
        var services = SetupCommonServices(httpClient, setupAuthSuccess: true);
        var provider = services.BuildServiceProvider();
        var customerService = provider.GetRequiredService<ICustomerService>();

        var customer = new CustomerDto
        {
            Id = Guid.NewGuid(),
            Name = "Test Customer",
            Email = "test@example.com"
        };

        // Act
        var result = await customerService.CreateCustomerAsync(customer);

        // Log the result for debugging
        if (!result)
        {
            var requestJson = System.Text.Json.JsonSerializer.Serialize(customer);
            TestOutputHelper.WriteLine($"Customer payload: {requestJson}");
            TestOutputHelper.WriteLine("Stubbed HTTP response: 200 OK, body: 'true'");
        }

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task UpdateCustomerAsync_ReturnsTrue_WhenApiReturnsSuccess()
    {
        // Arrange
        var httpClient = CreateMockHttpClient(HttpStatusCode.OK, "true");
        var services = SetupCommonServices(httpClient, setupAuthSuccess: true);
        var provider = services.BuildServiceProvider();
        var customerService = provider.GetRequiredService<ICustomerService>();

        var customer = new CustomerDto
        {
            Id = Guid.NewGuid(),
            Name = "Updated Customer",
            Email = "updated@example.com"
        };

        // Act
        var result = await customerService.UpdateCustomerAsync(customer.Id, customer);

        // Log the result for debugging
        if (!result)
        {
            var requestJson = System.Text.Json.JsonSerializer.Serialize(customer);
            TestOutputHelper.WriteLine($"Customer payload: {requestJson}");
            TestOutputHelper.WriteLine("Stubbed HTTP response: 200 OK, body: 'true'");
        }

        // Assert
        Assert.True(result);
    }
}
