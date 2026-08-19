using AutoMapper;
using Pollynx.Application.DTOs.Polls;
using Pollynx.Application.Interfaces;
using Pollynx.Domain.Entities;

namespace Pollynx.Application.Services;

public class PollService : IPollService
{
    private readonly IPollRepository _pollRepository;
    private readonly IMapper _mapper;

    public PollService(
        IPollRepository pollRepository,
        IMapper mapper)
    {
        _pollRepository = pollRepository;
        _mapper = mapper;
    }

    public async Task<PollResponseDto> CreateAsync(
        CreatePollDto request)
    {
        if (request.StartTime >= request.EndTime)
        {
            throw new ArgumentException(
                "Start time must be before end time.");
        }

        if (request.Options.Count < 2)
        {
            throw new ArgumentException(
                "A poll must have at least two options.");
        }

        var options = request.Options
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (options.Count < 2)
        {
            throw new ArgumentException(
                "A poll must have at least two unique options.");
        }

        var poll = new Poll
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            IsPublic = request.IsPublic,
            IsClosed = false,
            CreatedAt = DateTime.UtcNow,
            Options = options
                .Select(x => new PollOption
                {
                    OptionText = x
                })
                .ToList()
        };

        await _pollRepository.AddAsync(poll);

        return _mapper.Map<PollResponseDto>(poll);
    }

    public async Task<List<PollResponseDto>> GetAllAsync()
    {
        var polls = await _pollRepository.GetAllAsync();

        return _mapper.Map<List<PollResponseDto>>(polls);
    }

    public async Task<PollResponseDto> GetByIdAsync(int id)
    {
        var poll = await _pollRepository.GetByIdAsync(id);

        if (poll is null)
        {
            throw new KeyNotFoundException(
                $"Poll with ID {id} was not found.");
        }

        return _mapper.Map<PollResponseDto>(poll);
    }

    public async Task<PollResponseDto> UpdateAsync(
        int id,
        UpdatePollDto request)
    {
        var poll = await _pollRepository.GetByIdAsync(id);

        if (poll is null)
        {
            throw new KeyNotFoundException(
                $"Poll with ID {id} was not found.");
        }

        if (poll.IsClosed)
        {
            throw new InvalidOperationException(
                "A closed poll cannot be edited.");
        }

        if (request.StartTime >= request.EndTime)
        {
            throw new ArgumentException(
                "Start time must be before end time.");
        }

        poll.Title = request.Title.Trim();
        poll.Description = request.Description.Trim();
        poll.StartTime = request.StartTime;
        poll.EndTime = request.EndTime;
        poll.IsPublic = request.IsPublic;

        await _pollRepository.UpdateAsync(poll);

        return _mapper.Map<PollResponseDto>(poll);
    }

    public async Task DeleteAsync(int id)
    {
        var poll = await _pollRepository.GetByIdAsync(id);

        if (poll is null)
        {
            throw new KeyNotFoundException(
                $"Poll with ID {id} was not found.");
        }

        if (poll.Votes.Count > 0)
        {
            throw new InvalidOperationException(
                "A poll with votes cannot be deleted.");
        }

        await _pollRepository.DeleteAsync(poll);
    }

    public async Task CloseAsync(int id)
    {
        var poll = await _pollRepository.GetByIdAsync(id);

        if (poll is null)
        {
            throw new KeyNotFoundException(
                $"Poll with ID {id} was not found.");
        }

        if (poll.IsClosed)
        {
            throw new InvalidOperationException(
                "Poll is already closed.");
        }

        poll.IsClosed = true;

        await _pollRepository.UpdateAsync(poll);
    }

    public async Task<List<PollResponseDto>> GetActivePollsAsync()
    {
        var polls = await _pollRepository.GetActivePollsAsync();

        return _mapper.Map<List<PollResponseDto>>(polls);
    }
}