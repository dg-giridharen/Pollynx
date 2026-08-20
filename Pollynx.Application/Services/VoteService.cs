using Pollynx.Application.DTOs.Votes;
using Pollynx.Application.Interfaces;
using Pollynx.Domain.Entities;

namespace Pollynx.Application.Services;

public class VoteService : IVoteService
{
    private readonly IPollRepository _pollRepository;
    private readonly IVoteRepository _voteRepository;

    public VoteService(
        IPollRepository pollRepository,
        IVoteRepository voteRepository)
    {
        _pollRepository = pollRepository;
        _voteRepository = voteRepository;
    }

    public async Task CastVoteAsync(
        int pollId,
        int userId,
        VoteRequestDto request)
    {
        var poll = await _pollRepository.GetByIdAsync(pollId);

        if (poll is null)
        {
            throw new KeyNotFoundException(
                $"Poll with ID {pollId} was not found.");
        }

        var now = DateTime.UtcNow;

        if (poll.IsClosed)
        {
            throw new InvalidOperationException(
                "This poll is closed.");
        }

        if (now < poll.StartTime)
        {
            throw new InvalidOperationException(
                "This poll has not started yet.");
        }

        if (now > poll.EndTime)
        {
            throw new InvalidOperationException(
                "This poll has ended.");
        }

        var option = poll.Options
            .FirstOrDefault(x => x.Id == request.PollOptionId);

        if (option is null)
        {
            throw new InvalidOperationException(
                "The selected option does not belong to this poll.");
        }

        var alreadyVoted =
            await _voteRepository.HasUserVotedAsync(
                userId,
                pollId);

        if (alreadyVoted)
        {
            throw new InvalidOperationException(
                "You have already voted in this poll.");
        }

        var vote = new Vote
        {
            UserId = userId,
            PollId = pollId,
            PollOptionId = request.PollOptionId,
            CreatedAt = DateTime.UtcNow
        };

        await _voteRepository.AddAsync(vote);
    }
}