using Microsoft.EntityFrameworkCore;
using ModularMonolith.Domain.BusinessEvents;
using ECommerce.BusinessEvents.Persistence;
using ECommerce.BusinessEvents.Domain;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Linq;
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
                var schema = JSchema.Parse(schemaDefinition);
                ExtractMetadataFromSchema(schema, config, string.Empty);
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
        private void ExtractMetadataFromSchema(JSchema schema, MetadataExtractionConfig config, string fieldPath)
        {
            if (schema.Properties == null) return;

            foreach (var property in schema.Properties)
            {
                var propertyName = property.Key;
                var propertySchema = property.Value;
                var currentPath = string.IsNullOrEmpty(fieldPath) ? propertyName : $"{fieldPath}.{propertyName}";

                // Check if this property has x-metadata annotation
                if (HasMetadataAnnotation(propertySchema))
                {
                    config.FieldsToExtract.Add(currentPath);
                    config.FieldTypes[currentPath] = MapSchemaTypeToDataType(propertySchema);
                }

                // Recursively process nested objects
                if (propertySchema.Type == JSchemaType.Object && propertySchema.Properties?.Any() == true)
                {
                    ExtractMetadataFromSchema(propertySchema, config, currentPath);
                }
            }
        }

        /// <summary>
        /// Checks if a JSON schema property has the x-metadata custom annotation set to true.
        /// </summary>
        private bool HasMetadataAnnotation(JSchema propertySchema)
        {
            if (propertySchema.ExtensionData?.TryGetValue("x-metadata", out var metadataValue) == true)
            {
                return metadataValue.Type == JTokenType.Boolean && metadataValue.Value<bool>();
            }
            return false;
        }

        /// <summary>
        /// Maps JSON schema types to BusinessEventMetadata DataType values.
        /// Supports string, number, boolean, and date types.
        /// </summary>
        private string MapSchemaTypeToDataType(JSchema propertySchema)
        {
            return propertySchema.Type switch
            {
                JSchemaType.String when IsDateTimeFormat(propertySchema) => "date",
                JSchemaType.String => "string",
                JSchemaType.Number or JSchemaType.Integer => "number",
                JSchemaType.Boolean => "boolean",
                _ => "string" // Default fallback
            };
        }

        /// <summary>
        /// Determines if a string property represents a date/time value based on format annotations.
        /// </summary>
        private bool IsDateTimeFormat(JSchema propertySchema)
        {
            var format = propertySchema.Format;
            return format == "date-time" || format == "date" || format == "time";
        }
    }
}
