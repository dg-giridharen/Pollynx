using Pollynx.Domain.Entities;

namespace Pollynx.Application.Interfaces;

public interface IPollRepository
{
    Task<Poll?> GetByIdAsync(int id);

    Task<List<Poll>> GetAllAsync();

    Task<List<Poll>> GetActivePollsAsync();

    Task AddAsync(Poll poll);

    Task UpdateAsync(Poll poll);

    Task DeleteAsync(Poll poll);
}