using System.Threading.Tasks;

namespace ECommerce.Contracts.Interfaces;

public interface IBusinessEventService
{
    Task TrackEventAsync(
        string entityType,
        string entityId,
        string eventType,
        string actorId,
        string actorType,
        object entityData);
}
