using PactNet;
using PactNet.Output.Xunit;
using PactNet.Verifier;
using Xunit.Abstractions;

namespace ECommerce.Pact.ProviderTests;

public class CustomerApiProviderTests(CustomWebApplicationFactory factory, ITestOutputHelper output)
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory = factory;

    private readonly string _pactPath = Path.GetFullPath(
        Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "../../../../ECommerce.Pact.ConsumerTests/bin/debug/TestResults/pacts"
        )
    );
    private readonly HttpClient _client = factory.CreateClient();

    // Get the path to the generated pact file from the consumer tests

    [Fact]
    public async Task EnsureCustomerApiHonoursPactWithConsumer()
    {
        // Ensure the pact file exists before running the test
        string pactFilePath = Path.Combine(_pactPath, "ECommerce-CustomerAPI-Consumer-ECommerceAPI.json");
        Assert.True(File.Exists(pactFilePath), $"Pact file does not exist: {pactFilePath}");

        string? baseUriString = _client.BaseAddress?.ToString().TrimEnd('/');
        Assert.NotNull(baseUriString);
        var baseUri = new Uri(baseUriString);

        var config = new PactVerifierConfig
        {
            Outputters = [new XunitOutput(output)],
            LogLevel = PactLogLevel.Debug
        };

        IPactVerifier verifier = new PactVerifier(config);
    }
}
