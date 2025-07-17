using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

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
                columns: new[] { "EntityType", "Version", "CreatedDate", "SchemaDefinition" },
                values: new object[,]
                {
                    { "Customer", 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "{\n                \"$schema\": \"https://json-schema.org/draft/2020-12/schema\",\n                \"$id\": \"https://example.com/schemas/customer/v1\",\n                \"title\": \"Customer\",\n                \"description\": \"A customer entity\",\n                \"type\": \"object\",\n                \"properties\": {\n                    \"Id\": {\n                        \"type\": \"string\",\n                        \"description\": \"Unique customer identifier\"\n                    },\n                    \"Name\": {\n                        \"type\": \"string\",\n                        \"description\": \"Customer's name\"\n                    },\n                    \"Email\": {\n                        \"type\": \"string\",\n                        \"format\": \"email\",\n                        \"description\": \"Customer's email address\"\n                    },\n                    \"Phone\": {\n                        \"type\": \"string\",\n                        \"description\": \"Customer's phone number\"\n                    },\n                    \"DateOfBirth\": {\n                        \"type\": \"string\",\n                        \"format\": \"date-time\",\n                        \"description\": \"Customer's date of birth\"\n                    },\n                    \"CreatedAt\": {\n                        \"type\": \"string\",\n                        \"format\": \"date-time\",\n                        \"description\": \"When the customer was created\"\n                    },\n                    \"UpdatedAt\": {\n                        \"type\": \"string\",\n                        \"format\": \"date-time\",\n                        \"description\": \"When the customer was last updated\"\n                    }\n                },\n                \"required\": [\"Id\", \"Name\", \"Email\"],\n                \"additionalProperties\": false\n            }" },
                    { "Product", 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "{\n                \"$schema\": \"https://json-schema.org/draft/2020-12/schema\",\n                \"$id\": \"https://example.com/schemas/product/v1\",\n                \"title\": \"Product\",\n                \"description\": \"A product entity\",\n                \"type\": \"object\",\n                \"properties\": {\n                    \"Id\": {\n                        \"type\": \"string\",\n                        \"description\": \"Unique product identifier\"\n                    },\n                    \"Name\": {\n                        \"type\": \"string\",\n                        \"description\": \"Product name\"\n                    },\n                    \"Price\": {\n                        \"type\": \"number\",\n                        \"format\": \"decimal\",\n                        \"description\": \"Product price\"\n                    }\n                },\n                \"required\": [\"Id\", \"Name\", \"Price\"],\n                \"additionalProperties\": false\n            }" }
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
