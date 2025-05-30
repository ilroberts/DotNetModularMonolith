// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using ECommerce.Common;

namespace ECommerce.BusinessEvents.Infrastructure.Validators;

public interface IJsonSchemaValidator
{
    Result<Unit, string> Validate(string json, string schema);
}
