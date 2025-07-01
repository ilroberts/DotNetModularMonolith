using System.Net;
using System.Net.Http.Json;
using Xunit;
using Xunit.Abstractions;

namespace ECommerceApp.IntegrationTests;

public class OrdersApiTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public OrdersApiTests(ITestOutputHelper testOutputHelper)
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

    private async Task<string> CreateCustomerAsync(string token)
    {
        var customer = new { name = "Order Customer", email = $"ordercustomer_{Guid.NewGuid()}@example.com" };
        var request = new HttpRequestMessage(HttpMethod.Post, "/customers");
        request.Headers.Add("Authorization", $"Bearer {token}");
        request.Content = JsonContent.Create(customer);
        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return System.Text.Json.JsonDocument.Parse(content).RootElement.GetProperty("id").GetString();
    }

    private async Task<string> CreateProductAsync(string token)
    {
        var product = new { name = "Order Product", description = "For order test", price = 1.23M, stock = 10 };
        var request = new HttpRequestMessage(HttpMethod.Post, "/products");
        request.Headers.Add("Authorization", $"Bearer {token}");
        request.Content = JsonContent.Create(product);
        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return System.Text.Json.JsonDocument.Parse(content).RootElement.GetProperty("id").GetString();
    }

    [Fact]
    public async Task Get_Orders_ReturnsOk()
    {
        var token = await GenerateTokenAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/orders");
        request.Headers.Add("Authorization", $"Bearer {token}");
        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Create_Order_ReturnsOk()
    {
        var token = await GenerateTokenAsync();
        var customerId = await CreateCustomerAsync(token);
        var productId = await CreateProductAsync(token);

        // Convert customerId string to Guid
        if (Guid.TryParse(customerId, out var customerGuid))
        {
            // Create order items as a list
            var items = new[]
            {
                new { ProductId = productId, Quantity = 1 }
            };

            // Send the request with correct parameter format
            var url = $"/orders?customerId={customerGuid}";
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("Authorization", $"Bearer {token}");
            request.Content = JsonContent.Create(items);

            var response = await _client.SendAsync(request);

            // Log error details if the test fails
            if (response.StatusCode != HttpStatusCode.OK)
            {
                string content = await response.Content.ReadAsStringAsync();
                _testOutputHelper.WriteLine($"Expected 200 OK but got {(int)response.StatusCode} {response.StatusCode}. Response content: {content}");
                _testOutputHelper.WriteLine($"Request payload: {await request.Content.ReadAsStringAsync()}");
                _testOutputHelper.WriteLine($"URL: {url}");
            }

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        else
        {
            Assert.Fail("Failed to parse customerId as Guid");
        }
    }

    [Fact(Skip = "No PUT endpoint available for updating orders")]
    public async Task Update_Order_ReturnsOk()
    {
        // This test is skipped because there is no PUT endpoint for updating orders
        // in the OrderEndpoints.cs implementation

        // When a PUT endpoint is implemented, this test can be re-enabled and updated
    }

    [Fact]
    public async Task Get_Order_By_Id_ReturnsOk()
    {
        var token = await GenerateTokenAsync();
        var customerId = await CreateCustomerAsync(token);
        var productId = await CreateProductAsync(token);

        // Create an order first
        if (Guid.TryParse(customerId, out var customerGuid))
        {
            // Create order items
            var items = new[]
            {
                new { ProductId = productId, Quantity = 1 }
            };

            // Create the order
            var url = $"/orders?customerId={customerGuid}";
            var createRequest = new HttpRequestMessage(HttpMethod.Post, url);
            createRequest.Headers.Add("Authorization", $"Bearer {token}");
            createRequest.Content = JsonContent.Create(items);
            var createResponse = await _client.SendAsync(createRequest);
            createResponse.EnsureSuccessStatusCode();
            var createdContent = await createResponse.Content.ReadAsStringAsync();

            // Since the response might not contain the ID directly (returns "Order created!" message),
            // we'll get all orders and take the first one
            var getAllRequest = new HttpRequestMessage(HttpMethod.Get, "/orders");
            getAllRequest.Headers.Add("Authorization", $"Bearer {token}");
            var getAllResponse = await _client.SendAsync(getAllRequest);
            getAllResponse.EnsureSuccessStatusCode();

            var ordersContent = await getAllResponse.Content.ReadAsStringAsync();
            var doc = System.Text.Json.JsonDocument.Parse(ordersContent);

            // Get the first order ID if available
            if (doc.RootElement.GetArrayLength() > 0)
            {
                var id = doc.RootElement[0].GetProperty("id").GetString();

                // Now get the specific order
                var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/orders/{id}");
                getRequest.Headers.Add("Authorization", $"Bearer {token}");
                var getResponse = await _client.SendAsync(getRequest);

                if (getResponse.StatusCode != HttpStatusCode.OK)
                {
                    string content = await getResponse.Content.ReadAsStringAsync();
                    _testOutputHelper.WriteLine($"Expected 200 OK but got {(int)getResponse.StatusCode} {getResponse.StatusCode}. Response content: {content}");
                }

                Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            }
            else
            {
                Assert.Fail("No orders returned from GET /orders");
            }
        }
        else
        {
            Assert.Fail("Failed to parse customerId as Guid");
        }
    }
}
