namespace ECommerce.BusinessEvents.Domain;

public class BusinessEvent
{
    public Guid EventId { get; init; }
    public string EntityType { get; init; } = string.Empty;
    public string EntityId { get; init; } = string.Empty;
    public string EventType { get; init; } = string.Empty;
    public int SchemaVersion { get; init; }
    public DateTimeOffset EventTimestamp { get; init; }
    public string CorrelationId { get; init; } = string.Empty;
    public string ActorId { get; init; } = string.Empty;
    public string ActorType { get; init; }
    public string EntityData { get; init; }
}
