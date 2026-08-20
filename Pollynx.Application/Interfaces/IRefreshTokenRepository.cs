using Pollynx.Domain.Entities;

namespace Pollynx.Application.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token);

    Task AddAsync(RefreshToken refreshToken);

    Task UpdateAsync(RefreshToken refreshToken);
}