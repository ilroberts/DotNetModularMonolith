using System.Threading.Tasks;

namespace ECommerce.Contracts.Interfaces;

public interface IBusinessEventService
{
    Task TrackEventAsync(
        string entityType,
        int entityId,
        string eventType,
        string actorId,
        string actorType,
        object entityData);
}
