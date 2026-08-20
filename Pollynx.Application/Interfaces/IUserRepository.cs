using Pollynx.Domain.Entities;

namespace Pollynx.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);

    Task<User?> GetByEmailAsync(string email);

    Task AddAsync(User user);

    Task<bool> ExistsByEmailAsync(string email);
}