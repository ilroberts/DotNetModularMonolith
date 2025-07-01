// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace ECommerceApp.IntegrationTests;

public class ProductsApiTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly CustomWebApplicationFactory<global::ECommerceApp.Program> _factory;
    private readonly HttpClient _client;

    public ProductsApiTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _factory = new CustomWebApplicationFactory<global::ECommerceApp.Program>("FakeConnectionString");
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Get_Products_ReturnsOk()
    {
        // Arrange
        var user = new { user_name = "testuser" };
        var tokenResponse = await _client.PostAsJsonAsync("/admin/generateToken", user);
        tokenResponse.EnsureSuccessStatusCode();
        var token = await tokenResponse.Content.ReadAsStringAsync();
        token = token.Trim('"'); // Remove quotes if returned as JSON string

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
}
