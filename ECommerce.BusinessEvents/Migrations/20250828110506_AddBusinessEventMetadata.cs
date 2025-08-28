using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.BusinessEvents.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessEventMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BusinessEventMetadata",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    MetadataKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EntityId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MetadataValue = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DataType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessEventMetadata", x => new { x.EventId, x.MetadataKey });
                    table.ForeignKey(
                        name: "FK_BusinessEventMetadata_BusinessEvents_EventId",
                        column: x => x.EventId,
                        principalTable: "BusinessEvents",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "SchemaVersions",
                keyColumns: new[] { "EntityType", "Version" },
                keyValues: new object[] { "Customer", 1 },
                column: "SchemaDefinition",
                value: "{\n    \"$schema\": \"https://json-schema.org/draft/2020-12/schema\",\n    \"$id\": \"https://example.com/schemas/customer/v1\",\n    \"title\": \"Customer\",\n    \"description\": \"A customer entity\",\n    \"type\": \"object\",\n    \"properties\": {\n        \"Id\": {\n            \"type\": \"string\",\n            \"description\": \"Unique customer identifier\"\n        },\n        \"Name\": {\n            \"type\": \"string\",\n            \"description\": \"Customer's name\"\n        },\n        \"Email\": {\n            \"type\": \"string\",\n            \"format\": \"email\",\n            \"description\": \"Customer's email address\"\n        },\n        \"Phone\": {\n            \"type\": \"string\",\n            \"description\": \"Customer's phone number\"\n        },\n        \"DateOfBirth\": {\n            \"type\": \"string\",\n            \"format\": \"date-time\",\n            \"description\": \"Customer's date of birth\"\n        },\n        \"CreatedAt\": {\n            \"type\": \"string\",\n            \"format\": \"date-time\",\n            \"description\": \"When the customer was created\"\n        },\n        \"UpdatedAt\": {\n            \"type\": \"string\",\n            \"format\": \"date-time\",\n            \"description\": \"When the customer was last updated\"\n        }\n    },\n    \"required\": [\"Id\", \"Name\", \"Email\"],\n    \"additionalProperties\": false\n}\n\n");

            migrationBuilder.UpdateData(
                table: "SchemaVersions",
                keyColumns: new[] { "EntityType", "Version" },
                keyValues: new object[] { "Product", 1 },
                column: "SchemaDefinition",
                value: "{\n    \"$schema\": \"https://json-schema.org/draft/2020-12/schema\",\n    \"$id\": \"https://example.com/schemas/product/v1\",\n    \"title\": \"Product\",\n    \"description\": \"A product entity\",\n    \"type\": \"object\",\n    \"properties\": {\n        \"Id\": {\n            \"type\": \"string\",\n            \"description\": \"Unique product identifier\"\n        },\n        \"Name\": {\n            \"type\": \"string\",\n            \"description\": \"Product name\"\n        },\n        \"Price\": {\n            \"type\": \"number\",\n            \"format\": \"decimal\",\n            \"description\": \"Product price\"\n        }\n    },\n    \"required\": [\"Id\", \"Name\", \"Price\"],\n    \"additionalProperties\": false\n}\n\n");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessEventMetadata_Entity",
                table: "BusinessEventMetadata",
                columns: new[] { "EntityType", "EntityId", "MetadataKey" });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessEventMetadata_Key_Value",
                table: "BusinessEventMetadata",
                columns: new[] { "MetadataKey", "MetadataValue" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BusinessEventMetadata");

            migrationBuilder.UpdateData(
                table: "SchemaVersions",
                keyColumns: new[] { "EntityType", "Version" },
                keyValues: new object[] { "Customer", 1 },
                column: "SchemaDefinition",
                value: "{\n                \"$schema\": \"https://json-schema.org/draft/2020-12/schema\",\n                \"$id\": \"https://example.com/schemas/customer/v1\",\n                \"title\": \"Customer\",\n                \"description\": \"A customer entity\",\n                \"type\": \"object\",\n                \"properties\": {\n                    \"Id\": {\n                        \"type\": \"string\",\n                        \"description\": \"Unique customer identifier\"\n                    },\n                    \"Name\": {\n                        \"type\": \"string\",\n                        \"description\": \"Customer's name\"\n                    },\n                    \"Email\": {\n                        \"type\": \"string\",\n                        \"format\": \"email\",\n                        \"description\": \"Customer's email address\"\n                    },\n                    \"Phone\": {\n                        \"type\": \"string\",\n                        \"description\": \"Customer's phone number\"\n                    },\n                    \"DateOfBirth\": {\n                        \"type\": \"string\",\n                        \"format\": \"date-time\",\n                        \"description\": \"Customer's date of birth\"\n                    },\n                    \"CreatedAt\": {\n                        \"type\": \"string\",\n                        \"format\": \"date-time\",\n                        \"description\": \"When the customer was created\"\n                    },\n                    \"UpdatedAt\": {\n                        \"type\": \"string\",\n                        \"format\": \"date-time\",\n                        \"description\": \"When the customer was last updated\"\n                    }\n                },\n                \"required\": [\"Id\", \"Name\", \"Email\"],\n                \"additionalProperties\": false\n            }");

            migrationBuilder.UpdateData(
                table: "SchemaVersions",
                keyColumns: new[] { "EntityType", "Version" },
                keyValues: new object[] { "Product", 1 },
                column: "SchemaDefinition",
                value: "{\n                \"$schema\": \"https://json-schema.org/draft/2020-12/schema\",\n                \"$id\": \"https://example.com/schemas/product/v1\",\n                \"title\": \"Product\",\n                \"description\": \"A product entity\",\n                \"type\": \"object\",\n                \"properties\": {\n                    \"Id\": {\n                        \"type\": \"string\",\n                        \"description\": \"Unique product identifier\"\n                    },\n                    \"Name\": {\n                        \"type\": \"string\",\n                        \"description\": \"Product name\"\n                    },\n                    \"Price\": {\n                        \"type\": \"number\",\n                        \"format\": \"decimal\",\n                        \"description\": \"Product price\"\n                    }\n                },\n                \"required\": [\"Id\", \"Name\", \"Price\"],\n                \"additionalProperties\": false\n            }");
        }
    }
}
