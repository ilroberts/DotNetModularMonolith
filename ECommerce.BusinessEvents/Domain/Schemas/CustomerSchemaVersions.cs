// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using ModularMonolith.Domain.BusinessEvents;

namespace ECommerce.BusinessEvents.Domain.Schemas;

public static class CustomerSchemaVersions
{
    private static readonly SchemaVersion V1 = new()
    {
        EntityType = "Customer",
        Version = 1,
        SchemaDefinition = @"{
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
            }",
        CreatedDate = new DateTime(2024, 1, 1)
    };

    public static IEnumerable<SchemaVersion> All => [V1];
}
