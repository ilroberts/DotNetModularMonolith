using System.Text.Json;
using ModularMonolith.Domain.BusinessEvents;
using ECommerce.BusinessEvents.Persistence;
using Json.Schema;

namespace ECommerce.BusinessEvents.Services
{
    public class EventTrackingService(
        BusinessEventDbContext dbContext,
        SchemaRegistryService schemaRegistry)
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
            var serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null
            };

            // Serialize the entity data to JSON
            var json = JsonSerializer.Serialize(entityData, serializerOptions);
            using var jsonDocument = JsonDocument.Parse(json);

            var schema = JsonSchema.FromText(latestSchema.SchemaDefinition);
            var result = schema.Evaluate(jsonDocument, new EvaluationOptions
            {
                OutputFormat = OutputFormat.List,
                RequireFormatValidation = true
            });

            if (!result.IsValid)
                throw new InvalidOperationException("Entity data does not match schema");

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
