// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using ModularMonolith.Domain.BusinessEvents;
using System.IO;

namespace ECommerce.BusinessEvents.Domain.Schemas;

public static class CustomerSchemaVersions
{
    private static readonly SchemaVersion V1 = new()
    {
        EntityType = "Customer",
        Version = 1,
        SchemaDefinition = SchemaFileLoader.LoadEmbeddedSchema("ECommerce.BusinessEvents.Resources.Schemas.customer.v1.schema.json"),
        CreatedDate = new DateTime(2024, 1, 1)
    };

    public static IEnumerable<SchemaVersion> All => [V1];
}
