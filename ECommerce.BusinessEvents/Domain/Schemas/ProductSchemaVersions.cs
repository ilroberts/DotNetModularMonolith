using ModularMonolith.Domain.BusinessEvents;

namespace ECommerce.BusinessEvents.Domain.Schemas;

public static class ProductSchemaVersions
{
    private static readonly SchemaVersion V1 = new()
    {
        EntityType = "Product",
        Version = 1,
        SchemaDefinition = @"{
                ""$schema"": ""https://json-schema.org/draft/2020-12/schema"",
                ""$id"": ""https://example.com/schemas/product/v1"",
                ""title"": ""Product"",
                ""description"": ""A product entity"",
                ""type"": ""object"",
                ""properties"": {
                    ""Id"": {
                        ""type"": ""string"",
                        ""description"": ""Unique product identifier""
                    },
                    ""Name"": {
                        ""type"": ""string"",
                        ""description"": ""Product name""
                    },
                    ""Price"": {
                        ""type"": ""number"",
                        ""format"": ""decimal"",
                        ""description"": ""Product price""
                    }
                },
                ""required"": [""Id"", ""Name"", ""Price""],
                ""additionalProperties"": false
            }",
        CreatedDate = new DateTime(2024, 1, 1)
    };

    public static IEnumerable<SchemaVersion> All => [V1];
}
