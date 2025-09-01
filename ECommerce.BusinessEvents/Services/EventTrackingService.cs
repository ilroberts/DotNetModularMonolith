using System.Text.Json;
using ECommerce.BusinessEvents.Infrastructure.Validators;
using ECommerce.BusinessEvents.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ECommerce.BusinessEvents.Domain;
using ECommerce.Contracts.DTOs;
using ECommerce.Contracts.Interfaces;
using ECommerce.Common;
using ECommerce.BusinessEvents.Infrastructure;
using JsonFlatten;
using Newtonsoft.Json.Linq;

namespace ECommerce.BusinessEvents.Services
{
    public class EventTrackingService(
        BusinessEventDbContext dbContext,
        SchemaRegistryService schemaRegistry,
        IJsonSchemaValidator schemaValidator,
        ILogger<EventTrackingService> logger,
        ITransactionManager transactionManager) : IBusinessEventService, IEventRetrievalService
    {
        public async Task<Result<Unit, string>> TrackEventAsync(BusinessEventDto businessEventDto)
        {
            string entityType = businessEventDto.EntityType;
            object entityData = businessEventDto.EntityData;

            logger.LogInformation("Tracking business event for entity type: {EntityType}, Event Type: {EventType}, Entity ID: {EntityId}, Correlation ID: {CorrelationId}",
                entityType, businessEventDto.EventType, businessEventDto.EntityId, businessEventDto.CorrelationId);

            var latestSchema = await schemaRegistry.GetLatestSchemaAsync(entityType);
            if (latestSchema == null)
                return Result<Unit, string>.Failure($"No schema found for entity type '{entityType}'. Please register a schema first.");

            int schemaVersion = latestSchema.Version;
            var serializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = null };
            string json = JsonSerializer.Serialize(entityData, serializerOptions);

            logger.LogInformation("Serialized business event entity data: {Json}", json);

            var validationResult = schemaValidator.Validate(json, latestSchema.SchemaDefinition);
            if (!validationResult.IsSuccess)
            {
                logger.LogError("Schema validation failed for entity type {EntityType} with data: {Json}. Error: {Error}", entityType, json, validationResult.Error);
                return Result<Unit, string>.Failure(validationResult.Error!);
            }

            await transactionManager.BeginTransactionAsync();
            try
            {
                var businessEvent = new BusinessEvent
                {
                    EventId = businessEventDto.EventId == Guid.Empty ? Guid.NewGuid() : businessEventDto.EventId,
                    EntityType = businessEventDto.EntityType,
                    EntityId = businessEventDto.EntityId,
                    EventType = businessEventDto.EventType.ToString(),
                    SchemaVersion = schemaVersion,
                    EventTimestamp = businessEventDto.EventTimestamp == default ?
                        DateTimeOffset.UtcNow : businessEventDto.EventTimestamp,
                    CorrelationId = businessEventDto.CorrelationId,
                    ActorId = businessEventDto.ActorId,
                    ActorType = businessEventDto.ActorType.ToString(),
                    EntityData = json
                };

                // Save main business event
                dbContext.BusinessEvents.Add(businessEvent);
                await dbContext.SaveChangesAsync();

                // Extract and save metadata
                var metadataResult = await ExtractAndSaveMetadataAsync(
                    businessEvent.EventId,
                    businessEvent.EntityType,
                    businessEvent.EntityId,
                    json,
                    schemaVersion);

                if (!metadataResult.IsSuccess)
                {
                    await transactionManager.RollbackAsync();
                    return Result<Unit, string>.Failure($"Metadata save failed: {metadataResult.Error}");
                }

                await transactionManager.CommitAsync();
                logger.LogInformation("Successfully tracked business event {EventId} with metadata", businessEvent.EventId);

                return Result<Unit, string>.Success(new Unit());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to track business event for entity type {EntityType}", entityType);
                await transactionManager.RollbackAsync();
                return Result<Unit, string>.Failure($"Failed to track business event: {ex.Message}");
            }
            finally
            {
                await transactionManager.DisposeAsync();
            }
        }

        /// <summary>
        /// Extracts metadata from JSON data based on schema configuration and saves to BusinessEventMetadata table.
        /// </summary>
        private async Task<Result<Unit, string>> ExtractAndSaveMetadataAsync(
            Guid eventId,
            string entityType,
            string entityId,
            string jsonData,
            int schemaVersion)
        {
            try
            {
                logger.LogInformation("Adding metadata for event {EventId}, entity type {EntityType}, entity ID {EntityId}",
                    eventId, entityType, entityId);

                // Get metadata configuration from schema
                var metadataConfig = await schemaRegistry.GetMetadataConfigAsync(entityType, schemaVersion);

                if (!metadataConfig.HasMetadata)
                {
                    logger.LogDebug("No metadata fields configured for {EntityType} v{SchemaVersion}, skipping metadata extraction",
                        entityType, schemaVersion);
                    return Result<Unit, string>.Success(new Unit());
                }

                // Extract metadata fields from JSON
                var metadataEntries = ExtractMetadataFromJson(jsonData, metadataConfig);

                // Create BusinessEventMetadata entries (filtering already done in ExtractMetadataFromJson)
                var businessEventMetadata = metadataEntries
                    .Select(entry => new BusinessEventMetadata
                    {
                        EventId = eventId,
                        EntityType = entityType,
                        EntityId = entityId,
                        MetadataKey = entry.Key,
                        MetadataValue = entry.Value.Value!.ToString()!,
                        DataType = entry.Value.DataType
                    }).ToList();

                if (businessEventMetadata.Any())
                {
                    dbContext.BusinessEventMetadata.AddRange(businessEventMetadata);
                    await dbContext.SaveChangesAsync();

                    logger.LogDebug("Extracted and saved {Count} metadata fields for event {EventId}",
                        businessEventMetadata.Count, eventId);
                }

                return Result<Unit, string>.Success(new Unit());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to extract metadata for event {EventId}", eventId);
                return Result<Unit, string>.Failure($"Metadata extraction failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Extracts metadata values from JSON data using JsonFlatten for automatic flattening.
        /// Uses schema configuration to filter which fields should be extracted as metadata.
        /// Only includes fields that have actual data (not null or empty).
        /// </summary>
        private Dictionary<string, (object? Value, string DataType)> ExtractMetadataFromJson(
            string jsonData,
            MetadataExtractionConfig config)
        {
            var metadata = new Dictionary<string, (object? Value, string DataType)>();

            try
            {
                // Convert string to JObject for JsonFlatten
                var jObject = JObject.Parse(jsonData);

                // Use JsonFlatten to automatically flatten the JSON into dot notation
                var flattened = jObject.Flatten();

                // Get all field patterns that should be extracted based on schema configuration
                var fieldsToExtract = GetAllFieldsToExtract(config);

                foreach ((string? key, object? value) in flattened)
                {
                    // Check if this flattened key matches any of our configured metadata fields
                    if (ShouldExtractField(key, fieldsToExtract) &&
                        value != null &&
                        !string.IsNullOrWhiteSpace(value.ToString()))
                    {
                        var dataType = GetDataTypeForFlattenedKey(key, config);
                        metadata[key] = (value, dataType);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error parsing JSON for metadata extraction: {JsonData}", jsonData);
            }

            return metadata;
        }

        /// <summary>
        /// Gets all field patterns that should be extracted based on schema configuration.
        /// Includes both static fields and dynamic array fields.
        /// </summary>
        private HashSet<string> GetAllFieldsToExtract(MetadataExtractionConfig config)
        {
            var fieldsToExtract = new HashSet<string>();

            // Add static fields
            foreach (var field in config.FieldsToExtract)
            {
                fieldsToExtract.Add(field);
            }

            // Add array field patterns
            foreach (var arrayPath in config.ArrayPathsToExtract.Keys)
            {
                try
                {
                    // Parse the JSON string back to JsonElement for processing
                    var arrayItemSchemaJson = config.ArrayPathsToExtract[arrayPath];
                    using var schemaDoc = JsonDocument.Parse(arrayItemSchemaJson);
                    var arrayItemSchema = schemaDoc.RootElement;

                    // Add patterns for array items (e.g., PhoneNumbers[*].Number, PhoneNumbers[*].Prefix)
                    if (arrayItemSchema.TryGetProperty("properties", out var propertiesElement))
                    {
                        foreach (var property in propertiesElement.EnumerateObject())
                        {
                            if (property.Value.TryGetProperty("x-metadata", out var metadataFlag) &&
                                metadataFlag.ValueKind == JsonValueKind.True)
                            {
                                // Create pattern like "PhoneNumbers[*].Number"
                                fieldsToExtract.Add($"{arrayPath}[*].{property.Name}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Error parsing array schema for path {ArrayPath}", arrayPath);
                }
            }

            return fieldsToExtract;
        }

        /// <summary>
        /// Determines if a flattened key should be extracted based on configured field patterns.
        /// </summary>
        private bool ShouldExtractField(string flattenedKey, HashSet<string> fieldsToExtract)
        {
            // Check exact matches first
            if (fieldsToExtract.Contains(flattenedKey))
            {
                return true;
            }

            // Check pattern matches (e.g., PhoneNumbers[*].Number matches PhoneNumbers[0].Number)
            return (from pattern in fieldsToExtract where
                pattern.Contains("[*]")
                select pattern.Replace("[*]", @"\[\d+\]"))
                    .Any(regexPattern => System.Text.RegularExpressions.Regex
                        .IsMatch(flattenedKey, $"^{regexPattern}$"));
        }

        /// <summary>
        /// Gets the data type for a flattened key based on schema configuration.
        /// </summary>
        private string GetDataTypeForFlattenedKey(string flattenedKey, MetadataExtractionConfig config)
        {
            // Check if we have an exact match in field types
            if (config.FieldTypes.TryGetValue(flattenedKey, out var exactType))
            {
                return exactType;
            }

            // For array fields, try to match the pattern
            foreach (var arrayPath in config.ArrayPathsToExtract.Keys.Where(arrayPath => flattenedKey.StartsWith($"{arrayPath}[")))
            {
                try
                {
                    // Parse the JSON string back to JsonElement for processing
                    string arrayItemSchemaJson = config.ArrayPathsToExtract[arrayPath];
                    using var schemaDoc = JsonDocument.Parse(arrayItemSchemaJson);
                    var arrayItemSchema = schemaDoc.RootElement;

                    if (!arrayItemSchema.TryGetProperty("properties", out var propertiesElement))
                    {
                        continue;
                    }

                    // Extract the property name from the flattened key (e.g., "Number" from "PhoneNumbers[0].Number")
                    int lastDotIndex = flattenedKey.LastIndexOf('.');
                    if (lastDotIndex < 0)
                    {
                        continue;
                    }

                    string propertyName = flattenedKey.Substring(lastDotIndex + 1);

                    if (propertiesElement.TryGetProperty(propertyName, out var propertySchema))
                    {
                        return GetDataTypeFromSchemaElement(propertySchema);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Error parsing array schema for data type lookup in path {ArrayPath}", arrayPath);
                }
            }

            return "string"; // Default fallback
        }

        /// <summary>
        /// Gets data type from a schema element (similar to GetDataTypeFromSchema but for individual elements).
        /// </summary>
        private string GetDataTypeFromSchemaElement(JsonElement propertySchema)
        {
            if (!propertySchema.TryGetProperty("type", out var typeElement))
                return "string";

            if (typeElement.ValueKind != JsonValueKind.String)
            {
                return "string";
            }

            string? typeString = typeElement.GetString();
            return typeString?.ToLower() switch
            {
                "string" => IsDateTimeFormatElement(propertySchema) ? "date" : "string",
                "number" => "number",
                "integer" => "number",
                "boolean" => "boolean",
                _ => "string"
            };

        }

        /// <summary>
        /// Checks if a string property has a date/time format annotation (element version).
        /// </summary>
        private bool IsDateTimeFormatElement(JsonElement propertySchema)
        {
            if (!propertySchema.TryGetProperty("format", out var formatElement) ||
                formatElement.ValueKind != JsonValueKind.String)
            {
                return false;
            }

            var format = formatElement.GetString();
            return format is "date" or "date-time" or "time";
        }

        public async Task<List<BusinessEvent>> GetAllEventsAsync()
        {
            return await dbContext.BusinessEvents.ToListAsync();
        }

        public async Task<BusinessEvent?> GetEventByIdAsync(Guid eventId)
        {
            return await dbContext.BusinessEvents.FirstOrDefaultAsync(e => e.EventId == eventId);
        }

        public async Task<BusinessEvent?> GetPreviousEventAsync(BusinessEvent currentEvent)
        {
            return await dbContext.BusinessEvents
                .Where(e => e.EntityType == currentEvent.EntityType &&
                            e.EntityId == currentEvent.EntityId &&
                            e.EventTimestamp < currentEvent.EventTimestamp)
                .OrderByDescending(e => e.EventTimestamp)
                .FirstOrDefaultAsync();
        }
    }
}
