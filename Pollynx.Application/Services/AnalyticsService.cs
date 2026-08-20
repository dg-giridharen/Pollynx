using Pollynx.Application.DTOs.Analytics;
using Pollynx.Application.Interfaces;

namespace Pollynx.Application.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly IPollRepository _pollRepository;
    private readonly IVoteRepository _voteRepository;

    public AnalyticsService(
        IPollRepository pollRepository,
        IVoteRepository voteRepository)
    {
        _pollRepository = pollRepository;
        _voteRepository = voteRepository;
    }

    public async Task<PollResultDto> GetResultsAsync(int pollId)
    {
        var poll = await _pollRepository.GetByIdAsync(pollId);

        if (poll is null)
        {
            throw new KeyNotFoundException(
                $"Poll with ID {pollId} was not found.");
        }

        var votes = await _voteRepository.GetVotesByPollAsync(pollId);

        var totalVotes = votes.Count;

        var options = poll.Options
            .Select(option =>
            {
                var voteCount = votes.Count(
                    vote => vote.PollOptionId == option.Id);

                var percentage = totalVotes == 0
                    ? 0
                    : Math.Round(
                        (double)voteCount / totalVotes * 100,
                        2);

                return new PollOptionResultDto
                {
                    PollOptionId = option.Id,
                    OptionText = option.OptionText,
                    VoteCount = voteCount,
                    Percentage = percentage
                };
            })
            .ToList();

        return new PollResultDto
        {
            PollId = poll.Id,
            PollTitle = poll.Title,
            TotalVotes = totalVotes,
            Options = options
        };
    }

    public async Task<PollAnalyticsDto> GetAnalyticsAsync(
        int pollId)
    {
        var poll = await _pollRepository.GetByIdAsync(pollId);

        if (poll is null)
        {
            throw new KeyNotFoundException(
                $"Poll with ID {pollId} was not found.");
        }

        var votes = await _voteRepository.GetVotesByPollAsync(pollId);

        var totalVotes = votes.Count;

        var options = poll.Options
            .Select(option =>
            {
                var voteCount = votes.Count(
                    vote => vote.PollOptionId == option.Id);

                var percentage = totalVotes == 0
                    ? 0
                    : Math.Round(
                        (double)voteCount / totalVotes * 100,
                        2);

                return new PollOptionResultDto
                {
                    PollOptionId = option.Id,
                    OptionText = option.OptionText,
                    VoteCount = voteCount,
                    Percentage = percentage
                };
            })
            .ToList();

        var trends = votes
            .GroupBy(vote => vote.CreatedAt.Date)
            .OrderBy(group => group.Key)
            .Select(group => new VoteTrendDto
            {
                Date = group.Key,
                VoteCount = group.Count()
            })
            .ToList();

        return new PollAnalyticsDto
        {
            PollId = poll.Id,
            PollTitle = poll.Title,
            TotalVotes = totalVotes,
            Options = options,
            Trends = trends
        };
    }
}