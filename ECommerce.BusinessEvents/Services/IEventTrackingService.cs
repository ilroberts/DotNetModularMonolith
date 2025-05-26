using System.Collections.Generic;
using System.Threading.Tasks;
using ModularMonolith.Domain.BusinessEvents;

namespace ECommerce.BusinessEvents.Services
{
    public interface IEventTrackingService
    {
        Task TrackEventAsync(
            string entityType,
            string entityId,
            string eventType,
            string actorId,
            string actorType,
            object entityData);

        Task<List<BusinessEvent>> GetAllEventsAsync();
    }
}
