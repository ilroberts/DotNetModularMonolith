// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace ECommerceApp.IntegrationTests;

public class ProductsApiTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ProductsApiTests(ITestOutputHelper testOutputHelper)
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
    public async Task Get_Products_ReturnsOk()
    {
        // Arrange
        var token = await GenerateTokenAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/products");
        request.Headers.Add("Authorization", $"Bearer {token}");

        // Act
        var response = await _client.SendAsync(request);

        // Log the actual status code for debugging
        var actualStatusCode = response.StatusCode;
        if (actualStatusCode != HttpStatusCode.OK)
        {
            string content = await response.Content.ReadAsStringAsync();
            _testOutputHelper.WriteLine($"Expected 200 OK but got {(int)actualStatusCode} {actualStatusCode}. Response content: {content}");
        }

        // Assert
        Assert.Equal(HttpStatusCode.OK, actualStatusCode);
    }

    [Fact]
    public async Task Create_Product_ReturnsCreated()
    {
        // Arrange
        var token = await GenerateTokenAsync();
        var product = new
        {
            name = "Test Product",
            description = "A product for testing.",
            price = 9.99M,
            stock = 100
        };
        var request = new HttpRequestMessage(HttpMethod.Post, "/products");
        request.Headers.Add("Authorization", $"Bearer {token}");
        request.Content = JsonContent.Create(product);

        // Act
        var response = await _client.SendAsync(request);

        // Log the actual status code for debugging
        var actualStatusCode = response.StatusCode;
        if (actualStatusCode != HttpStatusCode.Created)
        {
            string content = await response.Content.ReadAsStringAsync();
            _testOutputHelper.WriteLine($"Expected 201 Created but got {(int)actualStatusCode} {actualStatusCode}. Response content: {content}");
        }

        // Assert
        Assert.Equal(HttpStatusCode.Created, actualStatusCode);
    }

    [Fact]
    public async Task Update_Product_ReturnsOk()
    {
        // Arrange
        var token = await GenerateTokenAsync();
        // First, create a product
        var product = new
        {
            name = "Product To Update",
            description = "Original description.",
            price = 5.00M,
            stock = 10
        };
        var createRequest = new HttpRequestMessage(HttpMethod.Post, "/products");
        createRequest.Headers.Add("Authorization", $"Bearer {token}");
        createRequest.Content = JsonContent.Create(product);
        var createResponse = await _client.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var createdContent = await createResponse.Content.ReadAsStringAsync();
        // Extract the product ID from the response (assuming it is returned as JSON with an id property as string)
        var id = System.Text.Json.JsonDocument.Parse(createdContent).RootElement.GetProperty("id").GetString();

        // Prepare update payload
        var updatedProduct = new
        {
            id,
            name = "Product To Update",
            description = "Updated description!",
            price = 7.50M,
            stock = 20
        };
        var updateRequest = new HttpRequestMessage(HttpMethod.Put, $"/products/{id}");
        updateRequest.Headers.Add("Authorization", $"Bearer {token}");
        updateRequest.Content = JsonContent.Create(updatedProduct);

        // Act
        var updateResponse = await _client.SendAsync(updateRequest);

        // Log the actual status code for debugging
        var actualStatusCode = updateResponse.StatusCode;
        if (actualStatusCode != HttpStatusCode.OK)
        {
            string content = await updateResponse.Content.ReadAsStringAsync();
            _testOutputHelper.WriteLine($"Expected 200 OK but got {(int)actualStatusCode} {actualStatusCode}. Response content: {content}");
        }

        // Assert
        Assert.Equal(HttpStatusCode.OK, actualStatusCode);
    }
}
