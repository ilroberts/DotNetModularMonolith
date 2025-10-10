using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.BusinessEvents.Migrations
{
    /// <inheritdoc />
    public partial class SeedCustomerAndProductSchemas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "SchemaVersions",
                columns: new[] { "EntityType", "Version", "SchemaDefinition", "CreatedDate" },
                values: new object[] { 
                    "Customer", 
                    1, 
                    "{\n    \"$schema\": \"https://json-schema.org/draft/2020-12/schema\",\n    \"$id\": \"https://example.com/schemas/customer/v1\",\n    \"title\": \"Customer\",\n    \"description\": \"A customer entity\",\n    \"type\": \"object\",\n    \"properties\": {\n        \"Id\": {\n            \"type\": \"string\",\n            \"description\": \"Unique customer identifier\",\n            \"x-metadata\": true\n        },\n        \"Name\": {\n            \"type\": \"string\",\n            \"description\": \"Customer's name\",\n            \"x-metadata\": true\n        },\n        \"Email\": {\n            \"type\": \"string\",\n            \"format\": \"email\",\n            \"description\": \"Customer's email address\",\n            \"x-metadata\": true\n        },\n        \"Phone\": {\n            \"type\": \"string\",\n            \"description\": \"Customer's phone number\"\n        },\n        \"DateOfBirth\": {\n            \"type\": \"string\",\n            \"format\": \"date-time\",\n            \"description\": \"Customer's date of birth\"\n        },\n        \"CreatedAt\": {\n            \"type\": \"string\",\n            \"format\": \"date-time\",\n            \"description\": \"When the customer was created\",\n            \"x-metadata\": true\n        },\n        \"UpdatedAt\": {\n            \"type\": \"string\",\n            \"format\": \"date-time\",\n            \"description\": \"When the customer was last updated\",\n            \"x-metadata\": true\n        }\n    },\n    \"required\": [\"Id\", \"Name\", \"Email\"],\n    \"additionalProperties\": false\n}\n",
                    new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                });

            migrationBuilder.InsertData(
                table: "SchemaVersions",
                columns: new[] { "EntityType", "Version", "SchemaDefinition", "CreatedDate" },
                values: new object[] { 
                    "Product", 
                    1, 
                    "{\n    \"$schema\": \"https://json-schema.org/draft/2020-12/schema\",\n    \"$id\": \"https://example.com/schemas/product/v1\",\n    \"title\": \"Product\",\n    \"description\": \"A product entity\",\n    \"type\": \"object\",\n    \"properties\": {\n        \"Id\": {\n            \"type\": \"string\",\n            \"description\": \"Unique product identifier\",\n            \"x-metadata\": true\n        },\n        \"Name\": {\n            \"type\": \"string\",\n            \"description\": \"Product name\",\n            \"x-metadata\": true\n        },\n        \"Price\": {\n            \"type\": \"number\",\n            \"format\": \"decimal\",\n            \"description\": \"Product price\",\n            \"x-metadata\": true\n        }\n    },\n    \"required\": [\"Id\", \"Name\", \"Price\"],\n    \"additionalProperties\": false\n}\n",
                    new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SchemaVersions",
                keyColumns: new[] { "EntityType", "Version" },
                keyValues: new object[] { "Customer", 1 });

            migrationBuilder.DeleteData(
                table: "SchemaVersions",
                keyColumns: new[] { "EntityType", "Version" },
                keyValues: new object[] { "Product", 1 });
        }
    }
}
