// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ECommerceApp.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace ECommerceApp.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtTokenService> _logger;
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;

    public JwtTokenService(IConfiguration configuration, ILogger<JwtTokenService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        _logger.LogInformation("All config values: {@Config}",
            configuration.GetSection("Jwt").GetChildren().Select(x => new { x.Key, x.Value }));

        _secret = _configuration["Jwt:Secret"] ?? string.Empty;
        _issuer = _configuration["Jwt:Issuer"] ?? string.Empty;
        _audience = _configuration["Jwt:Audience"] ?? string.Empty;
    }

    public string GenerateToken(string username)
    {
        if (string.IsNullOrEmpty(username))
        {
            throw new ArgumentException("Username cannot be null or empty", nameof(username));
        }

        _logger.LogInformation("Secret key for token is {Secret}", _secret);
        byte[] key = Encoding.ASCII.GetBytes(_secret);
        var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>()
        {
            new(JwtRegisteredClaimNames.Sub, username),
        };

        var securityToken = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(securityToken);
    }

    public bool ValidateToken(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return false;
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secret);

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                // Validate the token expiration
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero // No tolerance for expired tokens
            }, out var validatedToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            return false;
        }
    }

    public string RefreshToken(string oldToken)
    {
        if (string.IsNullOrEmpty(oldToken))
        {
            throw new ArgumentException("Token cannot be null or empty", nameof(oldToken));
        }

        try
        {
            // Extract claims from the old token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secret);

            // For refresh, we'll validate everything except lifetime
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = false // Don't validate lifetime for refresh operations
            };

            // Extract principal from the token
            var principal = tokenHandler.ValidateToken(oldToken, validationParameters, out var validatedToken);

            // Extract username from claims
            var usernameClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub);
            if (usernameClaim == null)
            {
                throw new Exception("Username claim not found in token");
            }

            // Generate a new token
            return GenerateToken(usernameClaim.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh token");
            throw new Exception("Token refresh failed", ex);
        }
    }
}
