using System.Threading.Tasks;
using ECommerce.Contracts.DTOs;

namespace ECommerce.Contracts.Interfaces;

public interface IBusinessEventService
{
    public enum ActorType
    {
        User,
        Admin,
        System
    }
    public enum EventType
    {
        Created,
        Updated,
        Deleted,
        Viewed
    }

    Task TrackEventAsync(BusinessEventDto businessEventDto);
}
