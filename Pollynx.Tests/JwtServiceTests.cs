using Microsoft.Extensions.Options;
using Pollynx.Application.DTOs.Auth;
using Pollynx.Domain.Entities;
using Pollynx.Domain.Enums;
using Pollynx.Infrastructure.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace Pollynx.Tests;

public class JwtServiceTests
{
    private static JwtService CreateService() =>
        new(Options.Create(new JwtSettings
        {
            Key = "super-secret-key-super-secret-key-super-secret-key",
            Issuer = "Pollynx",
            Audience = "Pollynx.Clients",
            AccessTokenExpirationMinutes = 30,
            RefreshTokenExpirationDays = 7
        }));

    [Fact]
    public void GenerateAccessToken_IncludesUserClaims()
    {
        var service = CreateService();
        var user = new User
        {
            Id = 42,
            FullName = "Jane Doe",
            Email = "jane@example.com",
            Role = UserRole.Admin
        };

        var token = service.GenerateAccessToken(user);
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        Assert.Equal("Pollynx", jwt.Issuer);
        Assert.Equal("Pollynx.Clients", jwt.Audiences.First());
        Assert.Equal("42", jwt.Claims
            .Single(c => c.Type == ClaimTypes.NameIdentifier).Value);
        Assert.Equal("jane@example.com", jwt.Claims
            .Single(c => c.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.Equal("Jane Doe", jwt.Claims
            .Single(c => c.Type == ClaimTypes.Name).Value);
        Assert.Equal(UserRole.Admin.ToString(), jwt.Claims
            .Single(c => c.Type == ClaimTypes.Role).Value);
    }

    [Fact]
    public void GenerateAccessToken_ExpiresAfterConfiguredMinutes()
    {
        var service = CreateService();
        var user = new User { Id = 1, FullName = "A", Email = "a@example.com" };

        var token = service.GenerateAccessToken(user);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        var expectedLifetime = TimeSpan.FromMinutes(30);
        Assert.True(jwt.ValidTo > DateTime.UtcNow);
        Assert.True(jwt.ValidTo <= DateTime.UtcNow.AddMinutes(31));
    }

    [Fact]
    public void GenerateRefreshToken_IsRandomAndLongEnough()
    {
        var service = CreateService();

        var first = service.GenerateRefreshToken();
        var second = service.GenerateRefreshToken();

        Assert.NotEqual(first, second);
        Assert.Equal(88, first.Length);
        var bytes = Convert.FromBase64String(first);
        Assert.Equal(64, bytes.Length);
    }
}