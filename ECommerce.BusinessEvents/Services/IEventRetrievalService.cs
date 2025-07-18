// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using ECommerce.BusinessEvents.Domain;

namespace ECommerce.BusinessEvents.Services;

public interface IEventRetrievalService
{
    Task<List<BusinessEvent>> GetAllEventsAsync();
    Task<BusinessEvent?> GetEventByIdAsync(Guid eventId);
    Task<BusinessEvent?> GetPreviousEventAsync(BusinessEvent currentEvent);
}
