using Microsoft.EntityFrameworkCore;
using Pollynx.Application.Interfaces;
using Pollynx.Domain.Entities;
using Pollynx.Infrastructure.Data;

namespace Pollynx.Infrastructure.Repositories;

public class PollRepository : IPollRepository
{
    private readonly ApplicationDbContext _context;

    public PollRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Poll?> GetByIdAsync(int id)
    {
        return await _context.Polls
            .Include(x => x.Options)
            .Include(x => x.Votes)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<Poll>> GetAllAsync()
    {
        return await _context.Polls
            .Include(x => x.Options)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Poll>> GetActivePollsAsync()
    {
        var now = DateTime.UtcNow;

        return await _context.Polls
            .Include(x => x.Options)
            .Where(x =>
                !x.IsClosed &&
                x.StartTime <= now &&
                x.EndTime >= now)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(Poll poll)
    {
        await _context.Polls.AddAsync(poll);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Poll poll)
    {
        _context.Polls.Update(poll);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Poll poll)
    {
        _context.Polls.Remove(poll);
        await _context.SaveChangesAsync();
    }
}