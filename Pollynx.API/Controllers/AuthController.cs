using Microsoft.AspNetCore.Mvc;
using Pollynx.Application.DTOs.Auth;
using Pollynx.Application.Interfaces;

namespace Pollynx.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        RegisterRequestDto request)
    {
        await _authService.RegisterAsync(request);

        return StatusCode(
            StatusCodes.Status201Created,
            new
            {
                message = "User registered successfully."
            });
    }

    [HttpPost("login")]
    [ProducesResponseType(
        typeof(LoginResponseDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        LoginRequestDto request)
    {
        var response =
            await _authService.LoginAsync(request);

        return Ok(response);
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType(
        typeof(LoginResponseDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken(
        RefreshTokenRequestDto request)
    {
        var response =
            await _authService.RefreshTokenAsync(request);

        return Ok(response);
    }
}