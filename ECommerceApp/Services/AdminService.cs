// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using ECommerceApp.Services.Interfaces;

namespace ECommerceApp.Services;

public class AdminService(ILogger<AdminService> logger, IJwtTokenService tokenService) : IAdminService
{
    public string GenerateToken(string username)
    {
        logger.LogInformation("Generating token for user: {Username}", username);
        return tokenService.GenerateToken(username);
    }
}
