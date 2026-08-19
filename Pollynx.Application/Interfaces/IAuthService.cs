using System;
using System.Collections.Generic;
using System.Text;

using Pollynx.Application.DTOs.Auth;

namespace Pollynx.Application.Interfaces;

public interface IAuthService
{
    Task RegisterAsync(RegisterRequestDto request);

    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);

    Task<LoginResponseDto> RefreshTokenAsync(
        RefreshTokenRequestDto request);
}
