namespace ECommerce.BusinessEvents.Domain
{
    public class BusinessEventResponse
    {
        public Guid EventId { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public DateTimeOffset EventTimestamp { get; set; }
        public string ActorId { get; set; } = string.Empty;
        public string ActorType { get; set; } = string.Empty;
        public Dictionary<string, string> Fields { get; set; } = new();
        public string? FullData { get; set; } // Only populated when no field selection
    }

    public class EventSearchRequest
    {
        public string EntityType { get; set; } = string.Empty;
        public string? EventType { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
        public string? Status { get; set; }
        public int? Limit { get; set; }
    }
}
