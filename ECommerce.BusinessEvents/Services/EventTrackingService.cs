using System.Text.Json;
using ECommerce.BusinessEvents.Infrastructure.Validators;
using ECommerce.BusinessEvents.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ECommerce.BusinessEvents.Domain;
using ECommerce.Contracts.DTOs;
using ECommerce.Contracts.Interfaces;

using ECommerce.Common;

namespace ECommerce.BusinessEvents.Services
{
    public class EventTrackingService(
        BusinessEventDbContext dbContext,
        SchemaRegistryService schemaRegistry,
        IJsonSchemaValidator schemaValidator,
        ILogger<EventTrackingService> logger) : IBusinessEventService, IEventRetrievalService
    {
        public async Task<Result<Unit, string>> TrackEventAsync(BusinessEventDto businessEventDto)
        {
            string entityType = businessEventDto.EntityType;
            object entityData = businessEventDto.EntityData;

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

            var businessEvent = new BusinessEvent
            {
                EventId = businessEventDto.EventId == Guid.Empty ? Guid.NewGuid() : businessEventDto.EventId,
                EntityType = businessEventDto.EntityType,
                EntityId = businessEventDto.EntityId,
                EventType = businessEventDto.EventType.ToString(),
                SchemaVersion = schemaVersion,
                EventTimestamp = businessEventDto.EventTimestamp == default ?
                    DateTimeOffset.UtcNow : businessEventDto.EventTimestamp,
                CorrelationId = string.IsNullOrEmpty(businessEventDto.CorrelationId) ?
                    Guid.NewGuid().ToString() : businessEventDto.CorrelationId,
                ActorId = businessEventDto.ActorId,
                ActorType = businessEventDto.ActorType.ToString(),
                EntityData = json
            };

            dbContext.BusinessEvents.Add(businessEvent);
            await dbContext.SaveChangesAsync();
            return Result<Unit, string>.Success(new Unit());
        }

        public async Task<List<BusinessEvent>> GetAllEventsAsync()
        {
            return await dbContext.BusinessEvents.ToListAsync();
        }
    }
}
