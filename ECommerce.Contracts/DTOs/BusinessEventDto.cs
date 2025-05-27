using System;
using ECommerce.Contracts.Interfaces;

namespace ECommerce.Contracts.DTOs;

public class BusinessEventDto
{
    public Guid EventId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public IBusinessEventService.EventType EventType { get; set; }
    public int SchemaVersion { get; set; }
    public DateTimeOffset EventTimestamp { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public string ActorId { get; set; } = string.Empty;
    public IBusinessEventService.ActorType ActorType { get; set; }
    public required object EntityData { get; set; }
}
