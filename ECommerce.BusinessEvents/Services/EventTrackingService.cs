using System.Text.Json;
using ECommerce.BusinessEvents.Infrastructure.Validators;
using ModularMonolith.Domain.BusinessEvents;
using ECommerce.BusinessEvents.Persistence;

namespace ECommerce.BusinessEvents.Services
{
    public class EventTrackingService(
        BusinessEventDbContext dbContext,
        SchemaRegistryService schemaRegistry,
        IJsonSchemaValidator schemaValidator)
    {
        public async Task TrackEventAsync(
            string entityType,
            int entityId,
            string eventType,
            string actorId,
            object entityData)
        {
            // Get the latest schema version for this entity type
            var latestSchema = await schemaRegistry.GetLatestSchemaAsync(entityType);
            if (latestSchema == null)
                throw new InvalidOperationException($"No schema found for entity type '{entityType}'. Please register a schema first.");

            // Use the latest schema version
            var schemaVersion = latestSchema.Version;

            // Serialize the entity data to JSON
            var serializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = null };
            var json = JsonSerializer.Serialize(entityData, serializerOptions);
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
                EntityData = json
            };

            dbContext.BusinessEvents.Add(businessEvent);
            await dbContext.SaveChangesAsync();
        }
    }
}
