using ECommerce.Contracts.Interfaces;

namespace ECommerce.BusinessEvents.Services;

public class BusinessEventService(IEventTrackingService eventTrackingService) : IBusinessEventService
{
    public Task TrackEventAsync(
        string entityType,
        string entityId,
        string eventType,
        string actorId,
        string actorType,
        object entityData)
    {
        return eventTrackingService.TrackEventAsync(
            entityType,
            entityId,
            eventType,
            actorId,
            actorType,
            entityData);
    }
}
