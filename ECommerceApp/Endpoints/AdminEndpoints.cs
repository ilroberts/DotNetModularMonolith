// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using ECommerceApp.Domain;
using ECommerceApp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Endpoints;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this WebApplication app)
    {
        app.MapPost("/admin/generateToken", (IAdminService adminService, User user) => adminService.GenerateToken(user.Name))
            .WithName("AdminFunctions")
            .WithTags("Admin");

        app.MapPost("/admin/refreshToken", (IJwtTokenService jwtTokenService, [FromHeader(Name = "Authorization")] string authorization) =>
        {
            // Extract token from Authorization header (Bearer token)
            if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return Results.Unauthorized();
            }

            var token = authorization[7..].Trim();

            try
            {
                var newToken = jwtTokenService.RefreshToken(token);
                return Results.Ok(newToken);
            }
            catch (Exception)
            {
                return Results.Unauthorized();
            }
        })
        .WithName("RefreshToken")
        .WithTags("Admin");
    }
}
