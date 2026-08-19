using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pollynx.Application.DTOs.Votes;
using Pollynx.Application.Interfaces;

namespace Pollynx.API.Controllers;

[ApiController]
[Route("api/polls/{pollId:int}/votes")]
[Authorize]
public class VotesController : ControllerBase
{
    private readonly IVoteService _voteService;

    public VotesController(IVoteService voteService)
    {
        _voteService = voteService;
    }

    [HttpPost]
    public async Task<IActionResult> Vote(
        int pollId,
        VoteRequestDto request)
    {
        var userIdClaim =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        await _voteService.CastVoteAsync(
            pollId,
            userId,
            request);

        return StatusCode(
            StatusCodes.Status201Created,
            new
            {
                message = "Vote cast successfully."
            });
    }
}