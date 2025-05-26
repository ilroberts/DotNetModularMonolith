using ECommerce.BusinessEvents.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace ECommerce.BusinessEvents.Endpoints
{
    public static class BusinessEventEndpoints
    {
        public static void MapBusinessEventEndpoints(this WebApplication app)
        {
            app.MapGet("/events", async (IEventTrackingService eventTrackingService) =>
                {
                    var events = await eventTrackingService.GetAllEventsAsync();
                    return Results.Ok(events);
                })
                .WithName("GetAllBusinessEvents")
                .WithTags("BusinessEvents")
                .RequireAuthorization();
        }
    }
}
