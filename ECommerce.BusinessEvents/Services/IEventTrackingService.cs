using System.Threading.Tasks;

namespace ECommerce.BusinessEvents.Services;

public interface IEventTrackingService
{
    Task TrackEventAsync(
        string entityType,
        string entityId,
        string eventType,
        string actorId,
        string actorType,
        object entityData);
}
