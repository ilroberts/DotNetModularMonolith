using Microsoft.EntityFrameworkCore;
using ECommerce.BusinessEvents.Persistence;
using ECommerce.BusinessEvents.Domain;
using System.Text.Json;
using DomainSchemaVersion = ModularMonolith.Domain.BusinessEvents.SchemaVersion;

namespace ECommerce.BusinessEvents.Services
{
    public class SchemaRegistryService(BusinessEventDbContext dbContext)
    {
        public async Task<DomainSchemaVersion?> GetSchemaAsync(string entityType, int version)
        {
            return await dbContext.SchemaVersions
                .FirstOrDefaultAsync(sv => sv.EntityType == entityType && sv.Version == version);
        }

        public async Task AddSchemaAsync(string entityType, int version, string schemaDefinition)
        {
            var schemaVersion = new DomainSchemaVersion
            {
                EntityType = entityType,
                Version = version,
                SchemaDefinition = schemaDefinition,
                CreatedDate = DateTime.UtcNow
            };

            dbContext.SchemaVersions.Add(schemaVersion);
            await dbContext.SaveChangesAsync();
        }

        public async Task<DomainSchemaVersion?> GetLatestSchemaAsync(string entityType)
        {
            return await dbContext.SchemaVersions
                .Where(sv => sv.EntityType == entityType)
                .OrderByDescending(sv => sv.Version)
                .FirstOrDefaultAsync();
        }

        public async Task<List<DomainSchemaVersion>> GetSchemaVersionsAsync(string entityType)
        {
            return await dbContext.SchemaVersions
                .Where(sv => sv.EntityType == entityType)
                .OrderBy(sv => sv.Version)
                .ToListAsync();
        }

        public async Task<string?> GetSchemaDefinitionAsync(string entityType, int version)
        {
            var schema = await GetSchemaAsync(entityType, version);
            return schema?.SchemaDefinition;
        }

        /// <summary>
        /// Extracts metadata configuration from a JSON schema based on x-metadata annotations.
        /// Returns configuration indicating which fields should be stored in BusinessEventMetadata table.
        /// </summary>
        public async Task<MetadataExtractionConfig> GetMetadataConfigAsync(string entityType, int version)
        {
            var schemaDefinition = await GetSchemaDefinitionAsync(entityType, version);

            if (string.IsNullOrEmpty(schemaDefinition))
            {
                return new MetadataExtractionConfig();
            }

            return ParseMetadataConfig(schemaDefinition);
        }

        /// <summary>
        /// Synchronous version of GetMetadataConfigAsync for cases where schema is already loaded.
        /// </summary>
        public MetadataExtractionConfig ParseMetadataConfig(string schemaDefinition)
        {
            var config = new MetadataExtractionConfig();

            try
            {
                using var document = JsonDocument.Parse(schemaDefinition);
                ExtractMetadataFromSchema(document.RootElement, config, string.Empty);
            }
            catch (Exception ex)
            {
                // Log the error but return empty config to allow graceful degradation
                // In a production system, you might want to use proper logging here
                Console.WriteLine($"Error parsing schema for metadata extraction: {ex.Message}");
            }

            return config;
        }

        /// <summary>
        /// Recursively extracts metadata field configurations from JSON schema.
        /// Supports nested objects using dot notation for field paths.
        /// Supports arrays with indexed notation for field paths (e.g., "PhoneNumbers[0].Number").
        /// </summary>
        private void ExtractMetadataFromSchema(JsonElement schemaElement, MetadataExtractionConfig config, string fieldPath)
        {
            if (!schemaElement.TryGetProperty("properties", out var propertiesElement))
                return;

            foreach (var property in propertiesElement.EnumerateObject())
            {
                var propertyName = property.Name;
                var propertySchema = property.Value;
                var currentPath = string.IsNullOrEmpty(fieldPath) ? propertyName : $"{fieldPath}.{propertyName}";

                // Check if this property is marked for metadata extraction
                if (propertySchema.TryGetProperty("x-metadata", out var metadataFlag) &&
                    metadataFlag.ValueKind == JsonValueKind.True)
                {
                    config.FieldsToExtract.Add(currentPath);

                    // Determine data type from schema
                    var dataType = GetDataTypeFromSchema(propertySchema);
                    config.FieldTypes[currentPath] = dataType;
                }

                if (propertySchema.TryGetProperty("type", out var typeElement) &&
                    typeElement.ValueKind == JsonValueKind.String)
                {
                    var typeString = typeElement.GetString();

                    switch (typeString)
                    {
                        // Recursively process nested objects
                        case "object":
                            ExtractMetadataFromSchema(propertySchema, config, currentPath);
                            break;
                        // Handle arrays - store the array path and item schema for dynamic extraction
                        case "array" when propertySchema.TryGetProperty("items", out var itemsElement):
                            {
                                // Check if array items are objects with properties that have metadata annotations
                                if (itemsElement.TryGetProperty("type", out var itemTypeElement) &&
                                    itemTypeElement.ValueKind == JsonValueKind.String &&
                                    itemTypeElement.GetString() == "object" &&
                                    itemsElement.TryGetProperty("properties", out _))
                                {
                                    // Store the array item schema as JSON string to avoid JsonElement disposal issues
                                    config.ArrayPathsToExtract[currentPath] = itemsElement.GetRawText();
                                }

                                break;
                            }
                    }
                }
            }
        }

        /// <summary>
        /// Determines the data type for metadata storage based on JSON schema type definition.
        /// </summary>
        private string GetDataTypeFromSchema(JsonElement propertySchema)
        {
            if (!propertySchema.TryGetProperty("type", out var typeElement))
                return "string";

            // Handle array of types (e.g., ["string", "null"])
            if (typeElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var typeString in from arrayElement in typeElement.EnumerateArray() where arrayElement.ValueKind == JsonValueKind.String select arrayElement.GetString() into typeString where typeString != "null" select typeString)
                {
                    return MapSchemaTypeToDataType(typeString, propertySchema);
                }

                return "string"; // Default fallback
            }

            // Handle single type
            if (typeElement.ValueKind == JsonValueKind.String)
            {
                var typeString = typeElement.GetString();
                return MapSchemaTypeToDataType(typeString, propertySchema);
            }

            return "string"; // Default fallback
        }

        /// <summary>
        /// Maps JSON schema types to metadata storage types.
        /// </summary>
        private string MapSchemaTypeToDataType(string? schemaType, JsonElement propertySchema)
        {
            return schemaType?.ToLower() switch
            {
                "string" => IsDateTimeFormat(propertySchema) ? "date" : "string",
                "number" => "number",
                "integer" => "number",
                "boolean" => "boolean",
                _ => "string"
            };
        }

        /// <summary>
        /// Checks if a string property has a date/time format annotation.
        /// </summary>
        private bool IsDateTimeFormat(JsonElement propertySchema)
        {
            if (propertySchema.TryGetProperty("format", out var formatElement) &&
                formatElement.ValueKind == JsonValueKind.String)
            {
                var format = formatElement.GetString();
                return format is "date" or "date-time" or "time";
            }
            return false;
        }
    }
}
