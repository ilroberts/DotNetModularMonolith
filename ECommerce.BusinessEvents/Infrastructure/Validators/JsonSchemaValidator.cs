using System.Text.Json;
using Json.Schema;
using Microsoft.Extensions.Logging;

namespace ECommerce.BusinessEvents.Infrastructure.Validators;

public class JsonSchemaValidator : IJsonSchemaValidator
{
    private readonly ILogger<JsonSchemaValidator> _logger;

    public JsonSchemaValidator(ILogger<JsonSchemaValidator> logger)
    {
        _logger = logger;
    }

    public void Validate(string json, string schemaDefinition)
    {
        using var jsonDocument = JsonDocument.Parse(json);
        var schema = JsonSchema.FromText(schemaDefinition);
        var result = schema.Evaluate(jsonDocument, new EvaluationOptions
        {
            OutputFormat = OutputFormat.List,
            RequireFormatValidation = true
        });

        if (!result.IsValid)
        {
            var errorMessages = string.Join("; ", result.Details
                .Where(r => !r.IsValid)
                .Select(r => r.ToString()));
            _logger.LogWarning("Schema validation failed. Errors: {Errors}. JSON: {Json}. Schema: {Schema}", errorMessages, json, schemaDefinition);
            throw new InvalidOperationException($"Validation failed: {errorMessages}");
        }
    }
}
