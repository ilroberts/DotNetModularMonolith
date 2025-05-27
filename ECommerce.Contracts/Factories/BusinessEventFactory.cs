// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using ECommerce.Contracts.DTOs;
using ECommerce.Contracts.Interfaces;

namespace ECommerce.Contracts.Factories;

public class BusinessEventFactory
{
    private readonly BusinessEventDto _dto = new() { EntityData = string.Empty }; // Ensure required property is set

    public static BusinessEventFactory Create()
        => new BusinessEventFactory();

    public BusinessEventFactory WithEntityType(string entityType)
    {
        _dto.EntityType = entityType;
        return this;
    }

    public BusinessEventFactory WithEntityId(string entityId)
    {
        _dto.EntityId = entityId;
        return this;
    }

    public BusinessEventFactory WithEventType(IBusinessEventService.EventType eventType)
    {
        _dto.EventType = eventType;
        return this;
    }

    public BusinessEventFactory WithActorId(string actorId)
    {
        _dto.ActorId = actorId;
        return this;
    }

    public BusinessEventFactory WithActorType(IBusinessEventService.ActorType actorType)
    {
        _dto.ActorType = actorType;
        return this;
    }

    public BusinessEventFactory WithSchemaVersion(int schemaVersion)
    {
        _dto.SchemaVersion = schemaVersion;
        return this;
    }

    public BusinessEventFactory WithEntityData(object entityData)
    {
        _dto.EntityData = entityData;
        return this;
    }

    public BusinessEventFactory WithEventTimestamp(DateTimeOffset? eventTimestamp)
    {
        _dto.EventTimestamp = eventTimestamp ?? DateTimeOffset.UtcNow;
        return this;
    }

    public BusinessEventFactory WithCorrelationId(string? correlationId)
    {
        _dto.CorrelationId = correlationId ?? Guid.NewGuid().ToString();
        return this;
    }

    public BusinessEventDto Build()
    {
        if (_dto.EventId == Guid.Empty)
            _dto.EventId = Guid.NewGuid();
        if (string.IsNullOrEmpty(_dto.CorrelationId))
            _dto.CorrelationId = Guid.NewGuid().ToString();
        if (_dto.EventTimestamp == default)
            _dto.EventTimestamp = DateTimeOffset.UtcNow;
        return _dto;
    }
}
