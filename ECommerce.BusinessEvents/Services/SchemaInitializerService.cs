using System;
using System.Threading.Tasks;
using ECommerce.BusinessEvents.Service;

namespace ECommerce.BusinessEvents.Services
{
    public class SchemaInitializerService
    {
        private readonly SchemaRegistryService _schemaRegistry;

        public SchemaInitializerService(SchemaRegistryService schemaRegistry)
        {
            _schemaRegistry = schemaRegistry;
        }

        public async Task InitializeDefaultSchemasAsync()
        {
            await InitializeCustomerSchemaAsync();
            // Add other entity schemas as needed
        }

        private async Task InitializeCustomerSchemaAsync()
        {
            const string entityType = "Customer";
            const int version = 1;

            var existingSchema = await _schemaRegistry.GetSchemaAsync(entityType, version);
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
                        ""type"": ""integer"",
                        ""description"": ""Unique customer identifier""
                    },
                    ""FirstName"": { 
                        ""type"": ""string"",
                        ""description"": ""Customer's first name""
                    },
                    ""LastName"": { 
                        ""type"": ""string"",
                        ""description"": ""Customer's last name""
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
                ""required"": [""Id"", ""FirstName"", ""LastName"", ""Email""],
                ""additionalProperties"": false
            }";

            await _schemaRegistry.AddSchemaAsync(entityType, version, customerSchema);
        }
    }
}
