using System;
using System.Threading.Tasks;
using System.Text.Json;
using ModularMonolith.Domain.BusinessEvents;
using ECommerce.BusinessEvents.Persistence;

namespace ECommerce.BusinessEvents.Service
{
    public class EventTrackingService
    {
        private readonly BusinessEventDbContext _dbContext;

        public EventTrackingService(BusinessEventDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task TrackEventAsync(
            string entityType,
            int entityId,
            string eventType,
            int schemaVersion,
            string actorId,
            object entityData)
        {
            string json = JsonSerializer.Serialize(entityData);

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

            _dbContext.BusinessEvents.Add(businessEvent);
            await _dbContext.SaveChangesAsync();
        }
    }
}
