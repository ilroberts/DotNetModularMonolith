using System;

namespace ECommerce.Contracts.DTOs;

public class BusinessEventDto
{
    public Guid EventId { get; set; }
    public string EntityType { get; set; }
    public int EntityId { get; set; }
    public string EventType { get; set; }
    public int SchemaVersion { get; set; }
    public DateTime EventTimestamp { get; set; }
    public string ActorId { get; set; }
    public string ActorType { get; set; }
    public string EntityData { get; set; }
}
