using ECommerce.BusinessEvents.Services;
using ECommerce.BusinessEvents.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.BusinessEvents.Endpoints
{
    public static class BusinessEventEndpoints
    {
        public static void MapBusinessEventEndpoints(this WebApplication app)
        {
            // Enhanced Events Endpoints with Metadata Support

            // Get events for a specific entity with optional field selection
            // Example: GET /events/Customer/123?fields=Name,Email,Status
            app.MapGet("/events/{entityType}/{entityId}", async (
                    [FromRoute] string entityType,
                    [FromRoute] string entityId,
                    [FromQuery] string? fields,
                    IEventQueryService eventQueryService) =>
                {
                    var fieldArray = string.IsNullOrEmpty(fields)
                        ? null
                        : fields.Split(',', StringSplitOptions.RemoveEmptyEntries);

                    var result = await eventQueryService.GetEntityEventsAsync(
                        entityType, entityId, fieldArray);

                    return result.IsSuccess
                        ? Results.Ok(result.Value)
                        : Results.BadRequest(result.Error);
                })
                .WithName("GetEntityEvents")
                .WithTags("BusinessEvents")
                .WithSummary("Get events for a specific entity with optional field selection")
                .WithDescription("Returns events for the specified entity. Use 'fields' query parameter to select specific metadata fields for better performance.")
                .RequireAuthorization();

            // Search events with metadata-based filtering
            // Example: GET /events/search?entityType=Customer&Email=*@example.com&eventType=Updated
            app.MapGet("/events/search", async (
                    [FromQuery] string entityType,
                    [FromQuery] string? eventType,
                    [FromQuery] string? email,
                    [FromQuery] string? name,
                    [FromQuery] string? status,
                    [FromQuery] int? limit,
                    IEventQueryService eventQueryService) =>
                {
                    if (string.IsNullOrEmpty(entityType))
                    {
                        return Results.BadRequest("EntityType is required for search operations");
                    }

                    var request = new EventSearchRequest
                    {
                        EntityType = entityType,
                        EventType = eventType,
                        Email = email,
                        Name = name,
                        Status = status,
                        Limit = limit
                    };

                    var result = await eventQueryService.SearchEventsAsync(request);

                    return result.IsSuccess
                        ? Results.Ok(result.Value)
                        : Results.BadRequest(result.Error);
                })
                .WithName("SearchEvents")
                .WithTags("BusinessEvents")
                .WithSummary("Search events with metadata-based filtering")
                .WithDescription("Search events using metadata fields with support for wildcards (e.g., Email=*@example.com)")
                .RequireAuthorization();

            // Original get all events endpoint
            app.MapGet("/events", async (IEventRetrievalService eventRetrieval) =>
                {
                    var events = await eventRetrieval.GetAllEventsAsync();
                    return Results.Ok(events);
                })
                .WithName("GetAllBusinessEvents")
                .WithTags("BusinessEvents")
                .WithSummary("Get all business events")
                .RequireAuthorization();

            // Schema Management Endpoints

            // Get all schema versions for an entity type
            app.MapGet("/schemas/{entityType}", async (
                    [FromRoute] string entityType,
                    SchemaRegistryService schemaRegistry) =>
                {
                    var schemas = await schemaRegistry.GetSchemaVersionsAsync(entityType);
                    if (schemas.Count == 0)
                        return Results.NotFound($"No schemas found for entity type '{entityType}'");

                    return Results.Ok(schemas);
                })
                .WithName("GetSchemaVersions")
                .WithTags("Schemas")
                .RequireAuthorization();

            // Get a specific schema version
            app.MapGet("/schemas/{entityType}/versions/{version:int}", async (
                    [FromRoute] string entityType,
                    [FromRoute] int version,
                    SchemaRegistryService schemaRegistry) =>
                {
                    var schema = await schemaRegistry.GetSchemaAsync(entityType, version);
                    if (schema == null)
                        return Results.NotFound($"Schema version {version} not found for entity type '{entityType}'");

                    return Results.Ok(schema);
                })
                .WithName("GetSchemaVersion")
                .WithTags("Schemas")
                .RequireAuthorization();

            // Get the latest schema for an entity type
            app.MapGet("/schemas/{entityType}/latest", async (
                    [FromRoute] string entityType,
                    SchemaRegistryService schemaRegistry) =>
                {
                    var schema = await schemaRegistry.GetLatestSchemaAsync(entityType);
                    if (schema == null)
                        return Results.NotFound($"No schemas found for entity type '{entityType}'");

                    return Results.Ok(schema);
                })
                .WithName("GetLatestSchema")
                .WithTags("Schemas")
                .RequireAuthorization();

            // Add a new schema version
            app.MapPost("/schemas", async (
                    SchemaVersionRequest request,
                    SchemaRegistryService schemaRegistry) =>
                {
                    // Validate the schema definition by trying to parse it
                    try
                    {
                        var jsonDocument = System.Text.Json.JsonDocument.Parse(request.SchemaDefinition);
                    }
                    catch (System.Text.Json.JsonException)
                    {
                        return Results.BadRequest("Invalid JSON schema definition");
                    }

                    // Check if this version already exists
                    var existingSchema = await schemaRegistry.GetSchemaAsync(request.EntityType, request.Version);
                    if (existingSchema != null)
                        return Results.Conflict($"Schema version {request.Version} already exists for entity type '{request.EntityType}'");

                    await schemaRegistry.AddSchemaAsync(request.EntityType, request.Version, request.SchemaDefinition);

                    return Results.Created($"/schemas/{request.EntityType}/versions/{request.Version}",
                        new { request.EntityType, request.Version });
                })
                .WithName("AddSchemaVersion")
                .WithTags("Schemas")
                .RequireAuthorization();

            // Generate a JSON Patch (git-style diff) between an event and its previous version
            app.MapGet("/events/{eventId:guid}/patch", async (
                [FromRoute] Guid eventId,
                IEventRetrievalService eventRetrieval) =>
            {
                var currentEvent = await eventRetrieval.GetEventByIdAsync(eventId);
                if (currentEvent == null)
                    return Results.NotFound($"Event with ID '{eventId}' not found");

                var previousEvent = await eventRetrieval.GetPreviousEventAsync(currentEvent);
                if (previousEvent == null)
                    return Results.NotFound($"No previous event found for entity '{currentEvent.EntityType}' with ID '{currentEvent.EntityId}'");

                var patch = JsonPatchUtility.GeneratePatch(previousEvent.EntityData, currentEvent.EntityData);
                return Results.Ok(patch.Operations);
            })
            .WithName("GetBusinessEventPatch")
            .WithTags("BusinessEvents")
            .RequireAuthorization();
        }
    }

    // Request DTO for new schema version
    public class SchemaVersionRequest
    {
        [Required]
        public string EntityType { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Version must be greater than 0")]
        public int Version { get; set; }

        [Required]
        public string SchemaDefinition { get; set; } = string.Empty;
    }
}
