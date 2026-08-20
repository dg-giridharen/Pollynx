using Microsoft.EntityFrameworkCore;
using Pollynx.Application.Interfaces;
using Pollynx.Domain.Entities;
using Pollynx.Infrastructure.Data;

namespace Pollynx.Infrastructure.Repositories;

public class VoteRepository : IVoteRepository
{
    private readonly ApplicationDbContext _context;

    public VoteRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Vote vote)
    {
        await _context.Votes.AddAsync(vote);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasUserVotedAsync(int userId, int pollId)
    {
        return await _context.Votes
            .AnyAsync(x =>
                x.UserId == userId &&
                x.PollId == pollId);
    }

    public async Task<int> GetTotalVotesAsync(int pollId)
    {
        return await _context.Votes
            .CountAsync(x => x.PollId == pollId);
    }

    public async Task<List<Vote>> GetVotesByPollAsync(int pollId)
    {
        return await _context.Votes
            .Where(x => x.PollId == pollId)
            .Include(x => x.PollOption)
            .ToListAsync();
    }
}