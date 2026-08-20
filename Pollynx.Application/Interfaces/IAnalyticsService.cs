using Pollynx.Application.DTOs.Analytics;

namespace Pollynx.Application.Interfaces;

public interface IAnalyticsService
{
    Task<PollResultDto> GetResultsAsync(int pollId);

    Task<PollAnalyticsDto> GetAnalyticsAsync(int pollId);
}