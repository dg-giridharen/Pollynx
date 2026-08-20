using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pollynx.Application.DTOs.Polls;
using Pollynx.Application.Interfaces;

namespace Pollynx.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PollsController : ControllerBase
{
    private readonly IPollService _pollService;

    public PollsController(IPollService pollService)
    {
        _pollService = pollService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var polls = await _pollService.GetAllAsync();

        return Ok(polls);
    }

    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<IActionResult> GetActive()
    {
        var polls = await _pollService.GetActivePollsAsync();

        return Ok(polls);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var poll = await _pollService.GetByIdAsync(id);

        return Ok(poll);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(
        typeof(PollResponseDto),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(
        CreatePollDto request)
    {
        var poll = await _pollService.CreateAsync(request);

        return CreatedAtAction(
            nameof(GetById),
            new { id = poll.Id },
            poll);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(
        int id,
        UpdatePollDto request)
    {
        var poll = await _pollService.UpdateAsync(
            id,
            request);

        return Ok(poll);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _pollService.DeleteAsync(id);

        return NoContent();
    }

    [HttpPost("{id:int}/close")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Close(int id)
    {
        await _pollService.CloseAsync(id);

        return Ok(new
        {
            message = "Poll closed successfully."
        });
    }
}