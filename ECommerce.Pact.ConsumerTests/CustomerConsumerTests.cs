using System.Net;
using System.Text;
using Newtonsoft.Json;
using PactNet;
using PactNet.Matchers;
using Xunit.Abstractions;

namespace ECommerce.Pact.ConsumerTests;

public class CustomerConsumerTests : PactConsumerTestBase, IDisposable
{
    private readonly HttpClient _httpClient;
    private const string AuthToken = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.test.token";

    public CustomerConsumerTests(ITestOutputHelper output) : base(output, "ECommerce-CustomerAPI-Consumer")
    {
        _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:9222") };
    }

    [Fact]
    public async Task CreateCustomer_ShouldReturnCreatedCustomer_WhenValidCustomerIsProvided()
    {
        // Arrange
        var customer = new
        {
            name = "John Doe",
            email = "john.doe@example.com"
        };

        var expectedResponse = new
        {
            id = Match.Type(Guid.NewGuid().ToString()),
            name = "John Doe",
            email = "john.doe@example.com"
        };

        PactBuilder
            .UponReceiving("A valid request to create a customer")
            .Given("No existing customers")
            .WithRequest(HttpMethod.Post, "/modulith/customers")
            .WithHeader("Content-Type", "application/json")
            .WithHeader("Authorization", AuthToken)
            .WithJsonBody(customer)
            .WillRespond()
            .WithStatus(HttpStatusCode.Created)
            .WithHeader("Content-Type", "application/json")
            .WithJsonBody(expectedResponse);

        // Act & Assert
        await PactBuilder.VerifyAsync(async ctx =>
        {
            _httpClient.BaseAddress = ctx.MockServerUri;

            var json = JsonConvert.SerializeObject(customer);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Add("Authorization", AuthToken);

            var response = await _httpClient.PostAsync("/modulith/customers", content);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseCustomer = JsonConvert.DeserializeObject<dynamic>(responseContent);

            Assert.NotNull(responseCustomer);
            Assert.Equal(customer.name, (string)responseCustomer!.name);
            Assert.Equal(customer.email, (string)responseCustomer.email);
            Assert.NotNull(responseCustomer.id);
        });
    }

    [Fact]
    public async Task CreateCustomer_ShouldReturnBadRequest_WhenInvalidCustomerIsProvided()
    {
        // Arrange
        var invalidCustomer = new
        {
            name = "",
            email = "invalid-email"
        };

        PactBuilder
            .UponReceiving("An invalid request to create a customer")
            .Given("No existing customers")
            .WithRequest(HttpMethod.Post, "/modulith/customers")
            .WithHeader("Content-Type", "application/json")
            .WithHeader("Authorization", AuthToken)
            .WithJsonBody(invalidCustomer)
            .WillRespond()
            .WithStatus(HttpStatusCode.BadRequest)
            .WithHeader("Content-Type", "application/json")
            .WithJsonBody(Match.Type("Validation error"));

        // Act & Assert
        await PactBuilder.VerifyAsync(async ctx =>
        {
            _httpClient.BaseAddress = ctx.MockServerUri;
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", AuthToken);

            var json = JsonConvert.SerializeObject(invalidCustomer);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/modulith/customers", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        });
    }

    [Fact]
    public async Task CreateCustomer_ShouldReturnUnauthorized_WhenNoAuthorizationProvided()
    {
        // Arrange
        var customer = new
        {
            name = "John Doe",
            email = "john.doe@example.com"
        };

        PactBuilder
            .UponReceiving("A request to create a customer without authorization")
            .Given("No existing customers")
            .WithRequest(HttpMethod.Post, "/modulith/customers")
            .WithHeader("Content-Type", "application/json")
            .WithJsonBody(customer)
            .WillRespond()
            .WithStatus(HttpStatusCode.Unauthorized);

        // Act & Assert
        await PactBuilder.VerifyAsync(async ctx =>
        {
            _httpClient.BaseAddress = ctx.MockServerUri;
            _httpClient.DefaultRequestHeaders.Clear();

            var json = JsonConvert.SerializeObject(customer);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/modulith/customers", content);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        });
    }

    [Fact]
    public async Task GetAllCustomers_ShouldReturnListOfCustomers_WhenCustomersExist()
    {
        // Arrange
        var expectedCustomers = new[]
        {
            new
            {
                id = Match.Type(Guid.NewGuid().ToString()),
                name = "John Doe",
                email = "john.doe@example.com"
            },
            new
            {
                id = Match.Type(Guid.NewGuid().ToString()),
                name = "Jane Smith",
                email = "jane.smith@example.com"
            }
        };

        PactBuilder
            .UponReceiving("A request to get all customers")
            .Given("Customers exist in the system")
            .WithRequest(HttpMethod.Get, "/modulith/customers")
            .WithHeader("Authorization", AuthToken)
            .WillRespond()
            .WithStatus(HttpStatusCode.OK)
            .WithHeader("Content-Type", "application/json")
            .WithJsonBody(expectedCustomers);

        // Act & Assert
        await PactBuilder.VerifyAsync(async ctx =>
        {
            _httpClient.BaseAddress = ctx.MockServerUri;
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", AuthToken);

            var response = await _httpClient.GetAsync("/modulith/customers");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var customers = JsonConvert.DeserializeObject<dynamic[]>(responseContent);

            Assert.NotNull(customers);
            Assert.Equal(2, customers.Length);
        });
    }

    [Fact]
    public async Task GetAllCustomers_ShouldReturnEmptyList_WhenNoCustomersExist()
    {
        // Arrange
        PactBuilder
            .UponReceiving("A request to get all customers when none exist")
            .Given("No customers exist in the system")
            .WithRequest(HttpMethod.Get, "/modulith/customers")
            .WithHeader("Authorization", AuthToken)
            .WillRespond()
            .WithStatus(HttpStatusCode.OK)
            .WithHeader("Content-Type", "application/json")
            .WithJsonBody(new object[0]);

        // Act & Assert
        await PactBuilder.VerifyAsync(async ctx =>
        {
            _httpClient.BaseAddress = ctx.MockServerUri;
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", AuthToken);

            var response = await _httpClient.GetAsync("/modulith/customers");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var customers = JsonConvert.DeserializeObject<dynamic[]>(responseContent);

            Assert.NotNull(customers);
            Assert.Empty(customers);
        });
    }

    [Fact]
    public async Task GetCustomerById_ShouldReturnCustomer_WhenCustomerExists()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var expectedCustomer = new
        {
            id = customerId.ToString(),
            name = "John Doe",
            email = "john.doe@example.com"
        };

        PactBuilder
            .UponReceiving("A request to get a customer by ID")
            .Given($"Customer with ID {customerId} exists")
            .WithRequest(HttpMethod.Get, $"/modulith/customers/{customerId}")
            .WithHeader("Authorization", AuthToken)
            .WillRespond()
            .WithStatus(HttpStatusCode.OK)
            .WithHeader("Content-Type", "application/json")
            .WithJsonBody(expectedCustomer);

        // Act & Assert
        await PactBuilder.VerifyAsync(async ctx =>
        {
            _httpClient.BaseAddress = ctx.MockServerUri;
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", AuthToken);

            var response = await _httpClient.GetAsync($"/modulith/customers/{customerId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var customer = JsonConvert.DeserializeObject<dynamic>(responseContent);

            Assert.NotNull(customer);
            Assert.Equal(expectedCustomer.id, (string)customer!.id);
            Assert.Equal(expectedCustomer.name, (string)customer.name);
            Assert.Equal(expectedCustomer.email, (string)customer.email);
        });
    }

    [Fact]
    public async Task GetCustomerById_ShouldReturnNotFound_WhenCustomerDoesNotExist()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        PactBuilder
            .UponReceiving("A request to get a customer by ID that doesn't exist")
            .Given($"Customer with ID {customerId} does not exist")
            .WithRequest(HttpMethod.Get, $"/modulith/customers/{customerId}")
            .WithHeader("Authorization", AuthToken)
            .WillRespond()
            .WithStatus(HttpStatusCode.NotFound);

        // Act & Assert
        await PactBuilder.VerifyAsync(async ctx =>
        {
            _httpClient.BaseAddress = ctx.MockServerUri;
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", AuthToken);

            var response = await _httpClient.GetAsync($"/modulith/customers/{customerId}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        });
    }

    [Fact]
    public async Task UpdateCustomer_ShouldReturnUpdatedCustomer_WhenValidUpdateIsProvided()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var updateRequest = new
        {
            name = "John Updated",
            email = "john.updated@example.com"
        };

        var expectedResponse = new
        {
            id = customerId.ToString(),
            name = "John Updated",
            email = "john.updated@example.com"
        };

        PactBuilder
            .UponReceiving("A valid request to update a customer")
            .Given($"Customer with ID {customerId} exists")
            .WithRequest(HttpMethod.Put, $"/modulith/customers/{customerId}")
            .WithHeader("Content-Type", "application/json")
            .WithHeader("Authorization", AuthToken)
            .WithJsonBody(updateRequest)
            .WillRespond()
            .WithStatus(HttpStatusCode.OK)
            .WithHeader("Content-Type", "application/json")
            .WithJsonBody(expectedResponse);

        // Act & Assert
        await PactBuilder.VerifyAsync(async ctx =>
        {
            _httpClient.BaseAddress = ctx.MockServerUri;
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", AuthToken);

            var json = JsonConvert.SerializeObject(updateRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"/modulith/customers/{customerId}", content);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var customer = JsonConvert.DeserializeObject<dynamic>(responseContent);

            Assert.NotNull(customer);
            Assert.Equal(updateRequest.name, (string)customer!.name);
            Assert.Equal(updateRequest.email, (string)customer.email);
            Assert.Equal(customerId.ToString(), (string)customer.id);
        });
    }

    [Fact]
    public async Task UpdateCustomer_ShouldReturnNotFound_WhenCustomerDoesNotExist()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var updateRequest = new
        {
            name = "John Updated",
            email = "john.updated@example.com"
        };

        PactBuilder
            .UponReceiving("A request to update a customer that doesn't exist")
            .Given($"Customer with ID {customerId} does not exist")
            .WithRequest(HttpMethod.Put, $"/modulith/customers/{customerId}")
            .WithHeader("Content-Type", "application/json")
            .WithHeader("Authorization", AuthToken)
            .WithJsonBody(updateRequest)
            .WillRespond()
            .WithStatus(HttpStatusCode.NotFound);

        // Act & Assert
        await PactBuilder.VerifyAsync(async ctx =>
        {
            _httpClient.BaseAddress = ctx.MockServerUri;
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", AuthToken);

            var json = JsonConvert.SerializeObject(updateRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"/modulith/customers/{customerId}", content);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        });
    }

    [Fact]
    public async Task GetAllCustomers_ShouldReturnUnauthorized_WhenNoAuthorizationProvided()
    {
        // Arrange
        PactBuilder
            .UponReceiving("A request to get all customers without authorization")
            .Given("Customers exist in the system")
            .WithRequest(HttpMethod.Get, "/modulith/customers")
            .WillRespond()
            .WithStatus(HttpStatusCode.Unauthorized);

        // Act & Assert
        await PactBuilder.VerifyAsync(async ctx =>
        {
            _httpClient.BaseAddress = ctx.MockServerUri;
            _httpClient.DefaultRequestHeaders.Clear();

            var response = await _httpClient.GetAsync("/modulith/customers");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        });
    }

    [Fact]
    public async Task GetCustomerById_ShouldReturnUnauthorized_WhenNoAuthorizationProvided()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        PactBuilder
            .UponReceiving("A request to get a customer by ID without authorization")
            .Given($"Customer with ID {customerId} exists")
            .WithRequest(HttpMethod.Get, $"/modulith/customers/{customerId}")
            .WillRespond()
            .WithStatus(HttpStatusCode.Unauthorized);

        // Act & Assert
        await PactBuilder.VerifyAsync(async ctx =>
        {
            _httpClient.BaseAddress = ctx.MockServerUri;
            _httpClient.DefaultRequestHeaders.Clear();

            var response = await _httpClient.GetAsync($"/modulith/customers/{customerId}");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        });
    }

    [Fact]
    public async Task UpdateCustomer_ShouldReturnUnauthorized_WhenNoAuthorizationProvided()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var updateRequest = new
        {
            name = "John Updated",
            email = "john.updated@example.com"
        };

        PactBuilder
            .UponReceiving("A request to update a customer without authorization")
            .Given($"Customer with ID {customerId} exists")
            .WithRequest(HttpMethod.Put, $"/modulith/customers/{customerId}")
            .WithHeader("Content-Type", "application/json")
            .WithJsonBody(updateRequest)
            .WillRespond()
            .WithStatus(HttpStatusCode.Unauthorized);

        // Act & Assert
        await PactBuilder.VerifyAsync(async ctx =>
        {
            _httpClient.BaseAddress = ctx.MockServerUri;
            _httpClient.DefaultRequestHeaders.Clear();

            var json = JsonConvert.SerializeObject(updateRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"/modulith/customers/{customerId}", content);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        });
    }

    [Fact]
    public async Task CreateCustomer_ShouldReturnBadRequest_WhenMissingRequiredFields()
    {
        // Arrange
        var invalidCustomer = new
        {
            name = (string?)null,
            email = (string?)null
        };

        PactBuilder
            .UponReceiving("A request to create a customer with missing required fields")
            .Given("No existing customers")
            .WithRequest(HttpMethod.Post, "/modulith/customers")
            .WithHeader("Content-Type", "application/json")
            .WithHeader("Authorization", AuthToken)
            .WithJsonBody(invalidCustomer)
            .WillRespond()
            .WithStatus(HttpStatusCode.BadRequest)
            .WithHeader("Content-Type", "application/json")
            .WithJsonBody(Match.Type("Validation error"));

        // Act & Assert
        await PactBuilder.VerifyAsync(async ctx =>
        {
            _httpClient.BaseAddress = ctx.MockServerUri;
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", AuthToken);

            var json = JsonConvert.SerializeObject(invalidCustomer);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/modulith/customers", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        });
    }

    [Fact]
    public async Task UpdateCustomer_ShouldReturnBadRequest_WhenInvalidDataProvided()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var invalidUpdateRequest = new
        {
            name = "",
            email = "not-an-email"
        };

        PactBuilder
            .UponReceiving("A request to update a customer with invalid data")
            .Given($"Customer with ID {customerId} exists")
            .WithRequest(HttpMethod.Put, $"/modulith/customers/{customerId}")
            .WithHeader("Content-Type", "application/json")
            .WithHeader("Authorization", AuthToken)
            .WithJsonBody(invalidUpdateRequest)
            .WillRespond()
            .WithStatus(HttpStatusCode.BadRequest)
            .WithHeader("Content-Type", "application/json")
            .WithJsonBody(Match.Type("Validation error"));

        // Act & Assert
        await PactBuilder.VerifyAsync(async ctx =>
        {
            _httpClient.BaseAddress = ctx.MockServerUri;
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", AuthToken);

            var json = JsonConvert.SerializeObject(invalidUpdateRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"/modulith/customers/{customerId}", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        });
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}
