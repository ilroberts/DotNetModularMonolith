using System.Text.Json;
using Json.Schema;
using Microsoft.Extensions.Logging;
using ECommerce.Common;

namespace ECommerce.BusinessEvents.Infrastructure.Validators;

public class JsonSchemaValidator(ILogger<JsonSchemaValidator> logger) : IJsonSchemaValidator
{
    public Result<Unit, string> Validate(string json, string schemaDefinition)
    {
        using var jsonDocument = JsonDocument.Parse(json);
        var schema = JsonSchema.FromText(schemaDefinition);
        var result = schema.Evaluate(jsonDocument, new EvaluationOptions
        {
            OutputFormat = OutputFormat.List,
            RequireFormatValidation = true
        });

        if (result.IsValid)
        {
            return Result<Unit, string>.Success(new Unit());
        }

        var errorMessages = string.Join("; ", result.Details
            .Where(r => !r.IsValid)
            .Select(r => r.ToString()));
        logger.LogWarning("Schema validation failed. Errors: {Errors}. JSON: {Json}. Schema: {Schema}", errorMessages, json, schemaDefinition);
        return Result<Unit, string>.Failure($"Validation failed: {errorMessages}");
    }
}
