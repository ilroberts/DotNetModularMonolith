using PactNet;
using PactNet.Output.Xunit;
using Xunit.Abstractions;
using static PactNet.Pact;

namespace ECommerce.Pact.ConsumerTests;

public abstract class PactConsumerTestBase
{
    protected IPactBuilderV3 PactBuilder { get; }

    protected PactConsumerTestBase(ITestOutputHelper output, string consumerName)
    {
        var config = new PactConfig
        {
           LogLevel = PactLogLevel.Debug,
           PactDir = "pacts",
           Outputters = [new XunitOutput(output)]
        };
        PactBuilder = V3(consumerName, "ECommerceAPI", config).WithHttpInteractions(9222);
    }
}
