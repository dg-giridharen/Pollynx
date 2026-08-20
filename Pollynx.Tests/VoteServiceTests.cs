using Moq;
using Pollynx.Application.DTOs.Votes;
using Pollynx.Application.Interfaces;
using Pollynx.Application.Services;
using Pollynx.Domain.Entities;
using Xunit;

namespace Pollynx.Tests;

public class VoteServiceTests
{
    private readonly Mock<IPollRepository> _pollRepository = new();
    private readonly Mock<IVoteRepository> _voteRepository = new();

    private VoteService CreateService() =>
        new(
            _pollRepository.Object,
            _voteRepository.Object);

    private static Poll OpenPoll(int optionId = 1) => new()
    {
        Id = 5,
        Title = "Test poll",
        Description = "desc",
        StartTime = DateTime.UtcNow.AddHours(-1),
        EndTime = DateTime.UtcNow.AddHours(1),
        IsPublic = true,
        IsClosed = false,
        Options = new List<PollOption>
        {
            new() { Id = optionId, PollId = 5, OptionText = "Yes" },
            new() { Id = optionId + 1, PollId = 5, OptionText = "No" }
        }
    };

    [Fact]
    public async Task CastVoteAsync_WithValidInput_AddsVote()
    {
        var poll = OpenPoll(optionId: 10);
        _pollRepository
            .Setup(x => x.GetByIdAsync(5))
            .ReturnsAsync(poll);
        _voteRepository
            .Setup(x => x.HasUserVotedAsync(7, 5))
            .ReturnsAsync(false);
        var service = CreateService();

        await service.CastVoteAsync(5, 7, new VoteRequestDto
        {
            PollOptionId = 10
        });

        _voteRepository.Verify(
            x => x.AddAsync(It.Is<Vote>(v =>
                v.UserId == 7 &&
                v.PollId == 5 &&
                v.PollOptionId == 10)),
            Times.Once);
    }

    [Fact]
    public async Task CastVoteAsync_DuplicateVote_Throws()
    {
        var poll = OpenPoll(optionId: 10);
        _pollRepository
            .Setup(x => x.GetByIdAsync(5))
            .ReturnsAsync(poll);
        _voteRepository
            .Setup(x => x.HasUserVotedAsync(7, 5))
            .ReturnsAsync(true);
        var service = CreateService();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CastVoteAsync(5, 7, new VoteRequestDto
            {
                PollOptionId = 10
            }));

        Assert.Contains("already voted", ex.Message);
    }

    [Fact]
    public async Task CastVoteAsync_PollNotFound_Throws()
    {
        _pollRepository
            .Setup(x => x.GetByIdAsync(99))
            .ReturnsAsync((Poll?)null);
        var service = CreateService();

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.CastVoteAsync(99, 7, new VoteRequestDto
            {
                PollOptionId = 1
            }));

        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public async Task CastVoteAsync_ClosedPoll_Throws()
    {
        var poll = OpenPoll(optionId: 10);
        poll.IsClosed = true;
        _pollRepository
            .Setup(x => x.GetByIdAsync(5))
            .ReturnsAsync(poll);
        var service = CreateService();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CastVoteAsync(5, 7, new VoteRequestDto
            {
                PollOptionId = 10
            }));

        Assert.Contains("closed", ex.Message);
    }

    [Fact]
    public async Task CastVoteAsync_PollNotStarted_Throws()
    {
        var poll = OpenPoll(optionId: 10);
        poll.StartTime = DateTime.UtcNow.AddHours(1);
        _pollRepository
            .Setup(x => x.GetByIdAsync(5))
            .ReturnsAsync(poll);
        var service = CreateService();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CastVoteAsync(5, 7, new VoteRequestDto
            {
                PollOptionId = 10
            }));

        Assert.Contains("not started", ex.Message);
    }

    [Fact]
    public async Task CastVoteAsync_PollEnded_Throws()
    {
        var poll = OpenPoll(optionId: 10);
        poll.EndTime = DateTime.UtcNow.AddHours(-1);
        _pollRepository
            .Setup(x => x.GetByIdAsync(5))
            .ReturnsAsync(poll);
        var service = CreateService();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CastVoteAsync(5, 7, new VoteRequestDto
            {
                PollOptionId = 10
            }));

        Assert.Contains("ended", ex.Message);
    }

    [Fact]
    public async Task CastVoteAsync_OptionNotInPoll_Throws()
    {
        var poll = OpenPoll(optionId: 10);
        _pollRepository
            .Setup(x => x.GetByIdAsync(5))
            .ReturnsAsync(poll);
        _voteRepository
            .Setup(x => x.HasUserVotedAsync(7, 5))
            .ReturnsAsync(false);
        var service = CreateService();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CastVoteAsync(5, 7, new VoteRequestDto
            {
                PollOptionId = 999
            }));

        Assert.Contains("does not belong", ex.Message);
    }
}