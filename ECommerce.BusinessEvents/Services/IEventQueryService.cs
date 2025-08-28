using ECommerce.Common;
using ECommerce.BusinessEvents.Domain;

namespace ECommerce.BusinessEvents.Services
{
    public interface IEventQueryService
    {
        Task<Result<List<BusinessEventResponse>, string>> GetEntityEventsAsync(
            string entityType, string entityId, string[] fields = null);
        Task<Result<List<BusinessEventResponse>, string>> SearchEventsAsync(
            EventSearchRequest request);
    }
}
