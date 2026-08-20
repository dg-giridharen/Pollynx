using Moq;
using Pollynx.Application.DTOs.Auth;
using Pollynx.Application.Interfaces;
using Pollynx.Application.Services;
using Pollynx.Domain.Entities;
using Pollynx.Domain.Enums;
using Xunit;

namespace Pollynx.Tests;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository = new();
    private readonly Mock<IJwtService> _jwtService = new();

    private AuthService CreateService() =>
        new(
            _userRepository.Object,
            _refreshTokenRepository.Object,
            _jwtService.Object);

    [Fact]
    public async Task RegisterAsync_ShouldCreateUserWithHashedPassword()
    {
        _userRepository
            .Setup(x => x.ExistsByEmailAsync("john@example.com"))
            .ReturnsAsync(false);
        var service = CreateService();

        await service.RegisterAsync(new RegisterRequestDto
        {
            FullName = "John Doe",
            Email = "  John@Example.com  ",
            Password = "Password@123"
        });

        _userRepository.Verify(
            x => x.AddAsync(It.Is<User>(u =>
                u.Email == "john@example.com" &&
                u.FullName == "John Doe" &&
                u.Role == UserRole.User &&
                BCrypt.Net.BCrypt.Verify("Password@123", u.PasswordHash))),
            Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_Throws()
    {
        _userRepository
            .Setup(x => x.ExistsByEmailAsync("john@example.com"))
            .ReturnsAsync(true);
        var service = CreateService();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RegisterAsync(new RegisterRequestDto
            {
                FullName = "John Doe",
                Email = "john@example.com",
                Password = "Password@123"
            }));

        Assert.Contains("already exists", ex.Message);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsTokens()
    {
        var user = new User
        {
            Id = 7,
            FullName = "John Doe",
            Email = "john@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password@123"),
            Role = UserRole.Admin
        };
        _userRepository
            .Setup(x => x.GetByEmailAsync("john@example.com"))
            .ReturnsAsync(user);
        _jwtService
            .Setup(x => x.GenerateAccessToken(user))
            .Returns("access-token");
        _jwtService
            .Setup(x => x.GenerateRefreshToken())
            .Returns("refresh-token");
        var service = CreateService();

        var response = await service.LoginAsync(new LoginRequestDto
        {
            Email = "  John@Example.com  ",
            Password = "Password@123"
        });

        Assert.Equal("access-token", response.AccessToken);
        Assert.Equal("refresh-token", response.RefreshToken);
        Assert.Equal(7, response.UserId);
        Assert.Equal(user.Role.ToString(), response.Role);
        _refreshTokenRepository.Verify(
            x => x.AddAsync(It.Is<RefreshToken>(t =>
                t.Token == "refresh-token" &&
                t.IsRevoked == false &&
                t.UserId == 7)),
            Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_Throws()
    {
        var user = new User
        {
            Id = 7,
            FullName = "John Doe",
            Email = "john@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Correct@123"),
            Role = UserRole.User
        };
        _userRepository
            .Setup(x => x.GetByEmailAsync("john@example.com"))
            .ReturnsAsync(user);
        var service = CreateService();

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.LoginAsync(new LoginRequestDto
            {
                Email = "john@example.com",
                Password = "Wrong@123"
            }));

        Assert.Contains("Invalid email or password", ex.Message);
    }

    [Fact]
    public async Task LoginAsync_WithUnknownEmail_Throws()
    {
        _userRepository
            .Setup(x => x.GetByEmailAsync("ghost@example.com"))
            .ReturnsAsync((User?)null);
        var service = CreateService();

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.LoginAsync(new LoginRequestDto
            {
                Email = "ghost@example.com",
                Password = "Password@123"
            }));

        Assert.Contains("Invalid email or password", ex.Message);
    }

    [Fact]
    public async Task RefreshTokenAsync_Success_RevokesOldToken()
    {
        var user = new User
        {
            Id = 7,
            FullName = "John Doe",
            Email = "john@example.com",
            Role = UserRole.User
        };
        var oldToken = new RefreshToken
        {
            Id = 1,
            UserId = 7,
            User = user,
            Token = "old-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = false
        };
        _refreshTokenRepository
            .Setup(x => x.GetByTokenAsync("old-refresh-token"))
            .ReturnsAsync(oldToken);
        _jwtService
            .Setup(x => x.GenerateAccessToken(user))
            .Returns("new-access-token");
        _jwtService
            .Setup(x => x.GenerateRefreshToken())
            .Returns("new-refresh-token");
        var service = CreateService();

        var response = await service.RefreshTokenAsync(
            new RefreshTokenRequestDto
            {
                RefreshToken = "old-refresh-token"
            });

        Assert.Equal("new-access-token", response.AccessToken);
        Assert.Equal("new-refresh-token", response.RefreshToken);
        Assert.True(oldToken.IsRevoked);
        _refreshTokenRepository.Verify(
            x => x.UpdateAsync(oldToken),
            Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_WithRevokedToken_Throws()
    {
        var oldToken = new RefreshToken
        {
            Id = 1,
            UserId = 7,
            Token = "revoked-token",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = true
        };
        _refreshTokenRepository
            .Setup(x => x.GetByTokenAsync("revoked-token"))
            .ReturnsAsync(oldToken);
        var service = CreateService();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.RefreshTokenAsync(new RefreshTokenRequestDto
            {
                RefreshToken = "revoked-token"
            }));
    }
}