// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using ECommerce.Modules.Customers.Domain;
using ECommerce.Modules.Customers.Persistence;

namespace ECommerce.Modules.Customers.Util;

public class SuspensionTypeSeedData
{
    public static void Seed(CustomerDbContext context)
    {
        context.SuspensionTypes.AddRange(new[]
        {
            new SuspensionType() { Code = "SUSPENSION_TYPE_1", Description = "Suspension type 1" },
            new SuspensionType() { Code = "SUSPENSION_TYPE_2", Description = "Suspension type 2" },
            new SuspensionType() { Code = "SUSPENSION_TYPE_3", Description = "Suspension type 3" },
        });
        context.SaveChanges();
    }
}
