// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;

namespace ECommerce.Modules.Customers.Domain;

public class SuspensionType
{
    public int Id { get; set; }

    [StringLength(10)]
    public string Code { get; set; } = string.Empty;

    [StringLength(255)]
    public string Description { get; set; } = string.Empty;
}
