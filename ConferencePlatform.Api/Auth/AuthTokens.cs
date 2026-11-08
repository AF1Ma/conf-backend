using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ConferencePlatform.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace ConferencePlatform.Api.Auth;

public sealed class AuthTokens
{
    private const string Issuer = "ScienceConferences";
    private const string Audience = "ScienceConferences.Web";
    private readonly SymmetricSecurityKey _key;

    public AuthTokens(string? secret)
    {
        // Without a configured key, sessions are valid until this process restarts.
        var key = string.IsNullOrWhiteSpace(secret) ? RandomNumberGenerator.GetBytes(32) : Encoding.UTF8.GetBytes(secret);
        if (key.Length < 32)
            throw new InvalidOperationException("Auth:SigningKey must be at least 32 bytes.");
        _key = new SymmetricSecurityKey(key);
    }

    public TokenValidationParameters ValidationParameters => new()
    {
        ValidateIssuer = true,
        ValidIssuer = Issuer,
        ValidateAudience = true,
        ValidAudience = Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = _key,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(30),
        NameClaimType = JwtRegisteredClaimNames.Sub,
        RoleClaimType = "role"
    };

    public string Issue(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim("role", user.Role)
        };
        var token = new JwtSecurityToken(Issuer, Audience, claims, DateTime.UtcNow,
            DateTime.UtcNow.AddHours(2), new SigningCredentials(_key, SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
