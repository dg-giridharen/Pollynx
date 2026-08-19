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
    public async Task<IActionResult> Login(
        LoginRequestDto request)
    {
        var response =
            await _authService.LoginAsync(request);

        return Ok(response);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken(
        RefreshTokenRequestDto request)
    {
        var response =
            await _authService.RefreshTokenAsync(request);

        return Ok(response);
    }
}