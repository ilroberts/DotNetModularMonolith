using Microsoft.EntityFrameworkCore;
using ECommerce.BusinessEvents.Persistence;
using ECommerce.BusinessEvents.Domain;
using ECommerce.Common;
using Microsoft.Extensions.Logging;

namespace ECommerce.BusinessEvents.Services
{
    public class EventQueryService : IEventQueryService
    {
        private readonly BusinessEventDbContext _context;
        private readonly ILogger<EventQueryService> _logger;

        public EventQueryService(BusinessEventDbContext context, ILogger<EventQueryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<List<BusinessEventResponse>, string>> GetEntityEventsAsync(
            string entityType, string entityId, string[] fields = null)
        {
            try
            {
                var query = _context.BusinessEvents
                    .Where(e => e.EntityType == entityType && e.EntityId == entityId)
                    .OrderByDescending(e => e.EventTimestamp);

                if (fields?.Any() == true)
                {
                    // Use metadata for field selection
                    var events = await query.ToListAsync();
                    var eventIds = events.Select(e => e.EventId).ToList();

                    var metadata = await _context.BusinessEventMetadata
                        .Where(m => eventIds.Contains(m.EventId) && fields.Contains(m.MetadataKey))
                        .ToListAsync();

                    var result = events.Select(evt => new BusinessEventResponse
                    {
                        EventId = evt.EventId,
                        EntityType = evt.EntityType,
                        EntityId = evt.EntityId,
                        EventType = evt.EventType,
                        EventTimestamp = evt.EventTimestamp,
                        ActorId = evt.ActorId,
                        ActorType = evt.ActorType,
                        Fields = metadata
                            .Where(m => m.EventId == evt.EventId)
                            .ToDictionary(m => m.MetadataKey, m => m.MetadataValue)
                    }).ToList();

                    _logger.LogDebug("Retrieved {EventCount} events with {FieldCount} fields for {EntityType}:{EntityId}",
                        result.Count, fields.Length, entityType, entityId);

                    return Result<List<BusinessEventResponse>, string>.Success(result);
                }
                else
                {
                    // Return full JSON data when no fields specified
                    var events = await query.ToListAsync();
                    var result = events.Select(evt => new BusinessEventResponse
                    {
                        EventId = evt.EventId,
                        EntityType = evt.EntityType,
                        EntityId = evt.EntityId,
                        EventType = evt.EventType,
                        EventTimestamp = evt.EventTimestamp,
                        ActorId = evt.ActorId,
                        ActorType = evt.ActorType,
                        FullData = evt.EntityData
                    }).ToList();

                    _logger.LogDebug("Retrieved {EventCount} full events for {EntityType}:{EntityId}",
                        result.Count, entityType, entityId);

                    return Result<List<BusinessEventResponse>, string>.Success(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve events for {EntityType}:{EntityId}", entityType, entityId);
                return Result<List<BusinessEventResponse>, string>.Failure(
                    $"Failed to retrieve events: {ex.Message}");
            }
        }

        public async Task<Result<List<BusinessEventResponse>, string>> SearchEventsAsync(
            EventSearchRequest request)
        {
            try
            {
                // Start with base event query
                var eventQuery = _context.BusinessEvents
                    .Where(e => e.EntityType == request.EntityType);

                if (!string.IsNullOrEmpty(request.EventType))
                {
                    eventQuery = eventQuery.Where(e => e.EventType == request.EventType);
                }

                // Apply metadata-based filters
                var metadataFilters = GetMetadataFilters(request);
                if (metadataFilters.Any())
                {
                    var filteredEventIds = await ApplyMetadataFilters(metadataFilters);
                    eventQuery = eventQuery.Where(e => filteredEventIds.Contains(e.EventId));
                }

                var events = await eventQuery
                    .OrderByDescending(e => e.EventTimestamp)
                    .Take(request.Limit ?? 100)
                    .ToListAsync();

                // Load metadata for results
                var eventIds = events.Select(e => e.EventId).ToList();
                var metadata = await _context.BusinessEventMetadata
                    .Where(m => eventIds.Contains(m.EventId))
                    .ToListAsync();

                var result = events.Select(evt => new BusinessEventResponse
                {
                    EventId = evt.EventId,
                    EntityType = evt.EntityType,
                    EntityId = evt.EntityId,
                    EventType = evt.EventType,
                    EventTimestamp = evt.EventTimestamp,
                    ActorId = evt.ActorId,
                    ActorType = evt.ActorType,
                    Fields = metadata
                        .Where(m => m.EventId == evt.EventId)
                        .ToDictionary(m => m.MetadataKey, m => m.MetadataValue)
                }).ToList();

                _logger.LogDebug("Search returned {EventCount} events for {EntityType} with {FilterCount} filters",
                    result.Count, request.EntityType, metadataFilters.Count);

                return Result<List<BusinessEventResponse>, string>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Search failed for entity type {EntityType}", request.EntityType);
                return Result<List<BusinessEventResponse>, string>.Failure(
                    $"Search failed: {ex.Message}");
            }
        }

        private async Task<List<Guid>> ApplyMetadataFilters(
            Dictionary<string, string> metadataFilters)
        {
            var eventIds = new List<Guid>();

            foreach (var filter in metadataFilters)
            {
                var query = _context.BusinessEventMetadata
                    .Where(m => m.MetadataKey == filter.Key);

                // Handle wildcard searches
                if (filter.Value.Contains("*"))
                {
                    var pattern = filter.Value.Replace("*", "%");
                    query = query.Where(m => EF.Functions.Like(m.MetadataValue, pattern));
                }
                else
                {
                    query = query.Where(m => m.MetadataValue == filter.Value);
                }

                var matchingEventIds = await query.Select(m => m.EventId).ToListAsync();

                if (!eventIds.Any())
                {
                    eventIds = matchingEventIds;
                }
                else
                {
                    // Intersection for AND logic
                    eventIds = eventIds.Intersect(matchingEventIds).ToList();
                }
            }

            return eventIds;
        }

        private Dictionary<string, string> GetMetadataFilters(EventSearchRequest request)
        {
            var filters = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(request.Email))
                filters["Email"] = request.Email;
            if (!string.IsNullOrEmpty(request.Name))
                filters["Name"] = request.Name;
            if (!string.IsNullOrEmpty(request.Status))
                filters["Status"] = request.Status;

            return filters;
        }
    }
}
