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

                // Check if this property has x-metadata annotation
                if (HasMetadataAnnotation(propertySchema))
                {
                    config.FieldsToExtract.Add(currentPath);
                    config.FieldTypes[currentPath] = MapSchemaTypeToDataType(propertySchema);
                }

                // Recursively process nested objects
                if (IsObjectType(propertySchema) && propertySchema.TryGetProperty("properties", out _))
                {
                    ExtractMetadataFromSchema(propertySchema, config, currentPath);
                }
            }
        }

        /// <summary>
        /// Checks if a JSON schema property has the x-metadata custom annotation set to true.
        /// </summary>
        private bool HasMetadataAnnotation(JsonElement propertySchema)
        {
            if (propertySchema.TryGetProperty("x-metadata", out var metadataElement))
            {
                return metadataElement.ValueKind == JsonValueKind.True;
            }
            return false;
        }

        /// <summary>
        /// Maps JSON schema types to BusinessEventMetadata DataType values.
        /// Supports string, number, boolean, and date types.
        /// </summary>
        private string MapSchemaTypeToDataType(JsonElement propertySchema)
        {
            if (!propertySchema.TryGetProperty("type", out var typeElement))
                return "string"; // Default fallback

            var schemaType = typeElement.GetString();

            return schemaType switch
            {
                "string" when IsDateTimeFormat(propertySchema) => "date",
                "string" => "string",
                "number" or "integer" => "number",
                "boolean" => "boolean",
                _ => "string" // Default fallback
            };
        }

        /// <summary>
        /// Determines if a string property represents a date/time value based on format annotations.
        /// </summary>
        private bool IsDateTimeFormat(JsonElement propertySchema)
        {
            if (propertySchema.TryGetProperty("format", out var formatElement))
            {
                var format = formatElement.GetString();
                return format == "date-time" || format == "date" || format == "time";
            }
            return false;
        }

        /// <summary>
        /// Determines if a JSON schema element represents an object type.
        /// </summary>
        private bool IsObjectType(JsonElement propertySchema)
        {
            if (propertySchema.TryGetProperty("type", out var typeElement))
            {
                return typeElement.GetString() == "object";
            }
            return false;
        }
    }
}
