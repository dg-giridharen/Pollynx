using Pollynx.Application.DTOs.Votes;

namespace Pollynx.Application.Interfaces;

public interface IVoteService
{
    Task CastVoteAsync(
        int pollId,
        int userId,
        VoteRequestDto request);
}