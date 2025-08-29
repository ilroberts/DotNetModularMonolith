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
        /// Extracts metadata values from JSON data based on configured field paths.
        /// Supports nested field access using dot notation.
        /// Only includes fields that have actual data (not null or empty).
        /// </summary>
        private Dictionary<string, (object? Value, string DataType)> ExtractMetadataFromJson(
            string jsonData,
            MetadataExtractionConfig config)
        {
            var metadata = new Dictionary<string, (object? Value, string DataType)>();

            try
            {
                using var document = JsonDocument.Parse(jsonData);
                var root = document.RootElement;

                foreach (var fieldPath in config.FieldsToExtract)
                {
                    var value = GetValueFromJsonPath(root, fieldPath);

                    // Only include fields that have actual data
                    if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
                    {
                        var dataType = config.FieldTypes.GetValueOrDefault(fieldPath, "string");
                        metadata[fieldPath] = (value, dataType);
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
        /// Retrieves a value from JSON using dot notation path with array indexing support (e.g., "PhoneNumbers[0].Number").
        /// </summary>
        private object? GetValueFromJsonPath(JsonElement element, string path)
        {
            var parts = path.Split('.');
            var current = element;

            foreach (var part in parts)
            {
                // Check if this part contains array indexing (e.g., "PhoneNumbers[0]")
                if (part.Contains('[') && part.Contains(']'))
                {
                    var arrayProperty = part.Substring(0, part.IndexOf('['));
                    var indexStr = part.Substring(part.IndexOf('[') + 1, part.IndexOf(']') - part.IndexOf('[') - 1);

                    if (!int.TryParse(indexStr, out var index))
                    {
                        return null;
                    }

                    // Navigate to the array property
                    if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(arrayProperty, out current))
                    {
                        return null;
                    }

                    // Navigate to the array element at the specified index
                    if (current.ValueKind != JsonValueKind.Array)
                    {
                        return null;
                    }

                    var arrayElements = current.EnumerateArray().ToArray();
                    if (index < 0 || index >= arrayElements.Length)
                    {
                        return null;
                    }

                    current = arrayElements[index];
                }
                else
                {
                    // Regular property access
                    if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(part, out current))
                    {
                        return null;
                    }
                }
            }

            return current.ValueKind switch
            {
                JsonValueKind.String => current.GetString(),
                JsonValueKind.Number => current.GetDecimal(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                _ => current.GetRawText()
            };
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
