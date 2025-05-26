using System.Text.Json;
using ECommerce.BusinessEvents.Infrastructure.Validators;
using ModularMonolith.Domain.BusinessEvents;
using ECommerce.BusinessEvents.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommerce.BusinessEvents.Services
{
    public class EventTrackingService(
        BusinessEventDbContext dbContext,
        SchemaRegistryService schemaRegistry,
        IJsonSchemaValidator schemaValidator,
        ILogger<EventTrackingService> logger) : IEventTrackingService
    {
        public async Task TrackEventAsync(
            string entityType,
            string entityId,
            string eventType,
            string actorId,
            string actorType,
            object entityData)
        {
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
                EventId = Guid.NewGuid(),
                EntityType = entityType,
                EntityId = entityId,
                EventType = eventType,
                SchemaVersion = schemaVersion,
                EventTimestamp = DateTime.UtcNow,
                ActorId = actorId,
                ActorType = actorType, // Set ActorType
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
