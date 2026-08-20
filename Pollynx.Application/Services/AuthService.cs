using BCrypt.Net;
using Pollynx.Application.DTOs.Auth;
using Pollynx.Application.Interfaces;
using Pollynx.Domain.Entities;
using Pollynx.Domain.Enums;

namespace Pollynx.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtService _jwtService;

    public AuthService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IJwtService jwtService)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtService = jwtService;
    }

    public async Task RegisterAsync(RegisterRequestDto request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        if (await _userRepository.ExistsByEmailAsync(normalizedEmail))
        {
            throw new InvalidOperationException(
                "A user with this email already exists.");
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
    }

    public async Task<LoginResponseDto> LoginAsync(
        LoginRequestDto request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await _userRepository
            .GetByEmailAsync(normalizedEmail);

        if (user is null)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        var passwordValid = BCrypt.Net.BCrypt.Verify(
            request.Password,
            user.PasswordHash);

        if (!passwordValid)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        return await CreateLoginResponseAsync(user);
    }

    public async Task<LoginResponseDto> RefreshTokenAsync(
        RefreshTokenRequestDto request)
    {
        var refreshToken =
            await _refreshTokenRepository
                .GetByTokenAsync(request.RefreshToken);

        if (refreshToken is null ||
            refreshToken.IsRevoked ||
            refreshToken.ExpiresAt <= DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException(
                "Invalid or expired refresh token.");
        }

        refreshToken.IsRevoked = true;

        await _refreshTokenRepository.UpdateAsync(refreshToken);

        return await CreateLoginResponseAsync(refreshToken.User);
    }

    private async Task<LoginResponseDto> CreateLoginResponseAsync(
        User user)
    {
        var accessToken =
            _jwtService.GenerateAccessToken(user);

        var refreshToken =
            _jwtService.GenerateRefreshToken();

        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };

        await _refreshTokenRepository.AddAsync(
            refreshTokenEntity);

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }
}