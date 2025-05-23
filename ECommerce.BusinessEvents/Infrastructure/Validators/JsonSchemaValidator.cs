using System.Text.Json;
using Json.Schema;

namespace ECommerce.BusinessEvents.Infrastructure.Validators;

public class JsonSchemaValidator : IJsonSchemaValidator
{
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
            throw new InvalidOperationException($"Validation failed: {errorMessages}");
        }
    }
}
