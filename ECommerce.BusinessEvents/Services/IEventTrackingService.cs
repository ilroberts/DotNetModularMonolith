using System.Threading.Tasks;

namespace ECommerce.BusinessEvents.Services;

public interface IEventTrackingService
{
    Task TrackEventAsync(
        string entityType,
        int entityId,
        string eventType,
        string actorId,
        string actorType,
        object entityData);
}
