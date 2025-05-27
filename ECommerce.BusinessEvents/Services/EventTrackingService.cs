using System.Text.Json;
using ECommerce.BusinessEvents.Infrastructure.Validators;
using ModularMonolith.Domain.BusinessEvents;
using ECommerce.BusinessEvents.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using ECommerce.BusinessEvents.Domain;
using ECommerce.Contracts.DTOs;
using ECommerce.Contracts.Interfaces;

namespace ECommerce.BusinessEvents.Services
{
    public class EventTrackingService(
        BusinessEventDbContext dbContext,
        SchemaRegistryService schemaRegistry,
        IJsonSchemaValidator schemaValidator,
        ILogger<EventTrackingService> logger) : IBusinessEventService, IEventRetrievalService
    {
        public async Task TrackEventAsync(BusinessEventDto businessEventDto)
        {
            string entityType = businessEventDto.EntityType;
            object entityData = businessEventDto.EntityData;

            // Get the latest schema version for this entity type
            var latestSchema = await schemaRegistry.GetLatestSchemaAsync(entityType);
            if (latestSchema == null)
                throw new InvalidOperationException($"No schema found for entity type '{entityType}'. Please register a schema first.");

            // Use the latest schema version
            int schemaVersion = latestSchema.Version;

            // Serialize the entity data to JSON
            var serializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = null };
            string json = JsonSerializer.Serialize(entityData, serializerOptions);

            logger.LogInformation("Serialized business event entity data: {Json}", json);

            schemaValidator.Validate(json, latestSchema.SchemaDefinition);

            var businessEvent = new BusinessEvent
            {
                EventId = businessEventDto.EventId == Guid.Empty ? Guid.NewGuid() : businessEventDto.EventId,
                EntityType = businessEventDto.EntityType,
                EntityId = businessEventDto.EntityId,
                EventType = businessEventDto.EventType.ToString(),
                SchemaVersion = businessEventDto.SchemaVersion,
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
        }

        public async Task<List<BusinessEvent>> GetAllEventsAsync()
        {
            return await dbContext.BusinessEvents.ToListAsync();
        }
    }
}
