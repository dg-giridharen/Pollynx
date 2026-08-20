using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pollynx.Application.DTOs.Analytics;
using Pollynx.Application.Interfaces;

namespace Pollynx.API.Controllers;

[ApiController]
[Route("api/polls/{pollId:int}")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(
        IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet("results")]
    [AllowAnonymous]
    [ProducesResponseType(
        typeof(PollResultDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetResults(int pollId)
    {
        var results =
            await _analyticsService.GetResultsAsync(pollId);

        return Ok(results);
    }

    [HttpGet("analytics")]
    [AllowAnonymous]
    [ProducesResponseType(
        typeof(PollAnalyticsDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAnalytics(int pollId)
    {
        var analytics =
            await _analyticsService.GetAnalyticsAsync(pollId);

        return Ok(analytics);
    }
}