using System.Threading.Tasks;

namespace ECommerce.Contracts.Interfaces;

public interface IBusinessEventService
{
    public enum ActorType
    {
        Customer,
        Admin,
        System
    }
    public enum EventType
    {
        Created,
        Updated,
        Deleted,
        Viewed
    }

    Task TrackEventAsync(
        string entityType,
        string entityId,
        EventType eventType,
        string actorId,
        ActorType actorType,
        object entityData);
}
