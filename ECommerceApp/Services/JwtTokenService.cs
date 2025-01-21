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
}
