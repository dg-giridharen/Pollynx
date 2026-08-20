using Pollynx.Domain.Entities;

namespace Pollynx.Application.Interfaces;

public interface IVoteRepository
{
    Task AddAsync(Vote vote);

    Task<bool> HasUserVotedAsync(int userId, int pollId);

    Task<int> GetTotalVotesAsync(int pollId);

    Task<List<Vote>> GetVotesByPollAsync(int pollId);
}