// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace ECommerce.BusinessEvents.Infrastructure.Validators;

public interface IJsonSchemaValidator
{
    void Validate(string json, string schema);
}
