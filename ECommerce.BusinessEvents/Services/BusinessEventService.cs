using ECommerce.Contracts.Interfaces;
using Microsoft.Extensions.Logging;

namespace ECommerce.BusinessEvents.Services;

public class BusinessEventService(
    IEventTrackingService eventTrackingService,
    ILogger<BusinessEventService> logger) : IBusinessEventService
{
    public async Task TrackEventAsync(
        string entityType,
        string entityId,
        IBusinessEventService.EventType eventType,
        string actorId,
        IBusinessEventService.ActorType actorType,
        object? entityData = null)
    {
        // Input validation
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType, nameof(entityType));
        ArgumentException.ThrowIfNullOrWhiteSpace(entityId, nameof(entityId));
        ArgumentException.ThrowIfNullOrWhiteSpace(actorId, nameof(actorId));

        if (!Enum.IsDefined(typeof(IBusinessEventService.EventType), eventType))
        {
            throw new ArgumentException($"Invalid event type: {eventType}", nameof(eventType));
        }

        if (!Enum.IsDefined(typeof(IBusinessEventService.ActorType), actorType))
        {
            throw new ArgumentException($"Invalid actor type: {actorType}", nameof(actorType));
        }

        try
        {
            logger.LogDebug("Tracking business event: {EventType} for {EntityType} {EntityId} by {ActorType} {ActorId}",
                eventType, entityType, entityId, actorType, actorId);

            // More efficient enum to string conversion
            string eventTypeString = eventType.ToString();
            string actorTypeString = actorType.ToString();

            await eventTrackingService.TrackEventAsync(
                entityType,
                entityId,
                eventTypeString,
                actorId,
                actorTypeString,
                entityData);

            logger.LogInformation("Successfully tracked business event: {EventType} for {EntityType} {EntityId}",
                eventType, entityType, entityId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to track business event: {EventType} for {EntityType} {EntityId}",
                eventType, entityType, entityId);
            throw;
        }
    }
}
