// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json.Serialization;

namespace ECommerceApp.Domain;

public class User
{
    [JsonPropertyName("user_name")]
    public required string Name { get; set; }
}
