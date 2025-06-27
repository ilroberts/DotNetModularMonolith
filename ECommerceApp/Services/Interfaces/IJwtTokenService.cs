// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace ECommerceApp.Services.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(string userId);
    bool ValidateToken(string token);
    string RefreshToken(string oldToken);
}
