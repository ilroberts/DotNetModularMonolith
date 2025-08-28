using Microsoft.AspNetCore.Mvc;
using ECommerce.BusinessEvents.Services;
using ECommerce.BusinessEvents.Domain;

namespace ECommerce.BusinessEvents.Endpoints
{
    [ApiController]
    [Route("api/events")]
    public class EventsController : ControllerBase
    {
        private readonly IEventQueryService _eventQueryService;

        public EventsController(IEventQueryService eventQueryService)
        {
            _eventQueryService = eventQueryService;
        }

        /// <summary>
        /// Get events for a specific entity with optional field selection.
        /// Example: GET /api/events/Customer/123?fields=Name,Email,Status
        /// </summary>
        [HttpGet("{entityType}/{entityId}")]
        public async Task<IActionResult> GetEntityEvents(
            string entityType,
            string entityId,
            [FromQuery] string fields = null)
        {
            var fieldArray = string.IsNullOrEmpty(fields)
                ? null
                : fields.Split(',', StringSplitOptions.RemoveEmptyEntries);

            var result = await _eventQueryService.GetEntityEventsAsync(
                entityType, entityId, fieldArray);

            return result.IsSuccess
                ? Ok(result.Value)
                : BadRequest(result.Error);
        }

        /// <summary>
        /// Search events with metadata-based filtering.
        /// Example: GET /api/events/search?entityType=Customer&Email=*@example.com&eventType=Updated
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchEvents(
            [FromQuery] EventSearchRequest request)
        {
            if (string.IsNullOrEmpty(request.EntityType))
            {
                return BadRequest("EntityType is required for search operations");
            }

            var result = await _eventQueryService.SearchEventsAsync(request);

            return result.IsSuccess
                ? Ok(result.Value)
                : BadRequest(result.Error);
        }
    }
}
