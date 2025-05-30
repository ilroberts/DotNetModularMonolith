using System.Threading.Tasks;
using ECommerce.Contracts.DTOs;
using ECommerce.Common;

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

    Task<Result<Unit, string>> TrackEventAsync(BusinessEventDto businessEventDto);
}
