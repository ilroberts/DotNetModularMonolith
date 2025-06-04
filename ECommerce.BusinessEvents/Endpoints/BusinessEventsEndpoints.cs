using ECommerce.BusinessEvents.Services;
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
            app.MapGet("/events", async (IEventRetrievalService eventRetrieval) =>
                {
                    var events = await eventRetrieval.GetAllEventsAsync();
                    return Results.Ok(events);
                })
                .WithName("GetAllBusinessEvents")
                .WithTags("BusinessEvents")
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
