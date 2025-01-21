// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using ECommerceApp.Domain;
using ECommerceApp.Services.Interfaces;

namespace ECommerceApp.Endpoints;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this WebApplication app)
    {
        app.MapPost("/admin/generateToken", (IAdminService adminService, User user) => adminService.GenerateToken(user.Name))
            .WithName("AdminFunctions")
            .WithTags("Admin");
    }
}
