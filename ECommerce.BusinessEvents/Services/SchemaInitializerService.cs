using Microsoft.Extensions.Logging;

namespace ECommerce.BusinessEvents.Services
{
    public class SchemaInitializerService(
        SchemaRegistryService schemaRegistry,
        ILogger<SchemaInitializerService> logger)
    {
        public async Task InitializeDefaultSchemasAsync()
        {
            await InitializeCustomerSchemaAsync();
            await InitializeProductSchemaAsync();
            // Add other entity schemas as needed
        }

        private async Task InitializeCustomerSchemaAsync()
        {
            const string entityType = "Customer";
            const int version = 1;

            var existingSchema = await schemaRegistry.GetSchemaAsync(entityType, version);
            if (existingSchema != null)
            {
                return; // Schema already exists
            }

            var customerSchema = @"{
                ""$schema"": ""https://json-schema.org/draft/2020-12/schema"",
                ""$id"": ""https://example.com/schemas/customer/v1"",
                ""title"": ""Customer"",
                ""description"": ""A customer entity"",
                ""type"": ""object"",
                ""properties"": {
                    ""Id"": {
                        ""type"": ""string"",
                        ""description"": ""Unique customer identifier""
                    },
                    ""Name"": {
                        ""type"": ""string"",
                        ""description"": ""Customer's name""
                    },
                    ""Email"": {
                        ""type"": ""string"",
                        ""format"": ""email"",
                        ""description"": ""Customer's email address""
                    },
                    ""Phone"": {
                        ""type"": ""string"",
                        ""description"": ""Customer's phone number""
                    },
                    ""DateOfBirth"": {
                        ""type"": ""string"",
                        ""format"": ""date-time"",
                        ""description"": ""Customer's date of birth""
                    },
                    ""CreatedAt"": {
                        ""type"": ""string"",
                        ""format"": ""date-time"",
                        ""description"": ""When the customer was created""
                    },
                    ""UpdatedAt"": {
                        ""type"": ""string"",
                        ""format"": ""date-time"",
                        ""description"": ""When the customer was last updated""
                    }
                },
                ""required"": [""Id"", ""Name"", ""Email""],
                ""additionalProperties"": false
            }";

            await schemaRegistry.AddSchemaAsync(entityType, version, customerSchema);
            logger.LogInformation("Initialized default schema for {EntityType} version {Version}", entityType, version);
        }

        private async Task InitializeProductSchemaAsync()
        {
            const string entityType = "Product";
            const int version = 1;

            var existingSchema = await schemaRegistry.GetSchemaAsync(entityType, version);
            if (existingSchema != null)
            {
                return; // Schema already exists
            }

            var productSchema = @"{
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
            }";

            await schemaRegistry.AddSchemaAsync(entityType, version, productSchema);
            logger.LogInformation("Initialized default schema for {EntityType} version {Version}", entityType, version);
        }
    }
}
