using System.Net;
using System.Net.Http.Json;
using Xunit;
using Xunit.Abstractions;

namespace ECommerceApp.IntegrationTests;

public class CustomersApiTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public CustomersApiTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _factory = new CustomWebApplicationFactory<Program>("FakeConnectionString");
        _client = _factory.CreateClient();
    }

    private async Task<string> GenerateTokenAsync()
    {
        var user = new { user_name = "testuser" };
        var tokenResponse = await _client.PostAsJsonAsync("/admin/generateToken", user);
        tokenResponse.EnsureSuccessStatusCode();
        var token = await tokenResponse.Content.ReadAsStringAsync();
        return token.Trim('"');
    }

    [Fact]
    public async Task Get_Customers_ReturnsOk()
    {
        var token = await GenerateTokenAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/customers");
        request.Headers.Add("Authorization", $"Bearer {token}");
        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Create_Customer_ReturnsCreated()
    {
        var token = await GenerateTokenAsync();
        var customer = new
        {
            name = "Test Customer",
            email = "testcustomer@example.com"
        };
        var request = new HttpRequestMessage(HttpMethod.Post, "/customers");
        request.Headers.Add("Authorization", $"Bearer {token}");
        request.Content = JsonContent.Create(customer);
        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Update_Customer_ReturnsOk()
    {
        var token = await GenerateTokenAsync();
        // First, create a customer
        var customer = new
        {
            name = "Customer To Update",
            email = "updatable@example.com"
        };
        var createRequest = new HttpRequestMessage(HttpMethod.Post, "/customers");
        createRequest.Headers.Add("Authorization", $"Bearer {token}");
        createRequest.Content = JsonContent.Create(customer);
        var createResponse = await _client.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var createdContent = await createResponse.Content.ReadAsStringAsync();
        var id = System.Text.Json.JsonDocument.Parse(createdContent).RootElement.GetProperty("id").GetString();

        // Prepare update payload
        var updatedCustomer = new
        {
            id,
            name = "Customer To Update",
            email = "updatedcustomer@example.com"
        };
        var updateRequest = new HttpRequestMessage(HttpMethod.Put, $"/customers/{id}");
        updateRequest.Headers.Add("Authorization", $"Bearer {token}");
        updateRequest.Content = JsonContent.Create(updatedCustomer);
        var updateResponse = await _client.SendAsync(updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
    }
}
