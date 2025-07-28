using ModularMonolith.Domain.BusinessEvents;

namespace ECommerce.BusinessEvents.Domain.Schemas;

public static class ProductSchemaVersions
{
    private static readonly SchemaVersion V1 = new()
    {
        EntityType = "Product",
        Version = 1,
        SchemaDefinition = SchemaFileLoader.LoadEmbeddedSchema("ECommerce.BusinessEvents.Resources.Schemas.product.v1.schema.json"),
        CreatedDate = new DateTime(2024, 1, 1)
    };

    public static IEnumerable<SchemaVersion> All => [V1];
}
