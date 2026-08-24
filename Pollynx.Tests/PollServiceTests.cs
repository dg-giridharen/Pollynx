using AutoMapper;
using Moq;
using Pollynx.Application.DTOs.Polls;
using Pollynx.Application.Interfaces;
using Pollynx.Application.Services;
using Pollynx.Domain.Entities;
using Xunit;

namespace Pollynx.Tests;

public class PollServiceTests
{
    private readonly Mock<IPollRepository> _pollRepository = new();
    private readonly Mock<IMapper> _mapper = new();

    private PollService CreateService() =>
        new(_pollRepository.Object, _mapper.Object);

    private void SetupMapping()
    {
        _mapper
            .Setup(m => m.Map<PollResponseDto>(It.IsAny<Poll>()))
            .Returns((Poll p) => new PollResponseDto
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                StartTime = p.StartTime,
                EndTime = p.EndTime,
                IsPublic = p.IsPublic,
                IsClosed = p.IsClosed
            });
    }

    private static CreatePollDto ValidCreateRequest() => new()
    {
        Title = "Favorite color",
        Description = "Pick one",
        StartTime = DateTime.UtcNow.AddHours(-1),
        EndTime = DateTime.UtcNow.AddHours(1),
        IsPublic = true,
        Options = new List<string> { "Red", "Blue", "Green" }
    };

    [Fact]
    public async Task CreateAsync_WithValidRequest_CreatesPoll()
    {
        SetupMapping();
        var service = CreateService();

        var result = await service.CreateAsync(ValidCreateRequest());

        Assert.Equal("Favorite color", result.Title);
        Assert.False(result.IsClosed);
        _pollRepository.Verify(
            x => x.AddAsync(It.Is<Poll>(p =>
                p.Title == "Favorite color" &&
                p.Options.Count == 3 &&
                p.IsClosed == false)),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_StartNotBeforeEnd_Throws()
    {
        var request = ValidCreateRequest();
        request.StartTime = DateTime.UtcNow.AddHours(2);
        request.EndTime = DateTime.UtcNow.AddHours(1);
        var service = CreateService();

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateAsync(request));

        Assert.Contains("before end time", ex.Message);
    }

    public static IEnumerable<object[]> InvalidOptionSets()
    {
        yield return new object[] { new[] { "Red" } };
        yield return new object[] { new[] { "Red", "Red" } };
        yield return new object[] { Array.Empty<string>() };
    }

    [Theory]
    [MemberData(nameof(InvalidOptionSets))]
    public async Task CreateAsync_InsufficientUniqueOptions_Throws(
        string[] options)
    {
        var request = ValidCreateRequest();
        request.Options = options.ToList();
        var service = CreateService();

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateAsync(request));

        Assert.Contains("two", ex.Message);
    }

    [Fact]
    public async Task CreateAsync_TrimsAndDeduplicatesOptions()
    {
        var request = ValidCreateRequest();
        request.Options = new List<string>
        {
            "  Red  ",
            " red ",
            "Blue",
            ""
        };
        var service = CreateService();

        await service.CreateAsync(request);

        _pollRepository.Verify(
            x => x.AddAsync(It.Is<Poll>(p =>
                p.Options.Count == 2 &&
                p.Options.Any(o => o.OptionText == "Red") &&
                p.Options.Any(o => o.OptionText == "Blue"))),
            Times.Once);
    }

    [Fact]
    public async Task CloseAsync_CloseClosedPoll_Throws()
    {
        var poll = new Poll
        {
            Id = 5,
            IsClosed = true
        };
        _pollRepository
            .Setup(x => x.GetByIdAsync(5))
            .ReturnsAsync(poll);
        var service = CreateService();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CloseAsync(5));

        Assert.Contains("already closed", ex.Message);
    }

    [Fact]
    public async Task CloseAsync_OpenPoll_Closes()
    {
        var poll = new Poll { Id = 5, IsClosed = false };
        _pollRepository
            .Setup(x => x.GetByIdAsync(5))
            .ReturnsAsync(poll);
        var service = CreateService();

        await service.CloseAsync(5);

        Assert.True(poll.IsClosed);
        _pollRepository.Verify(
            x => x.UpdateAsync(poll),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_PollWithVotes_Throws()
    {
        var poll = new Poll
        {
            Id = 5,
            Votes = new List<Vote>
            {
                new() { Id = 1 },
                new() { Id = 2 }
            }
        };
        _pollRepository
            .Setup(x => x.GetByIdAsync(5))
            .ReturnsAsync(poll);
        var service = CreateService();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.DeleteAsync(5));

        Assert.Contains("votes", ex.Message);
    }

    [Fact]
    public async Task DeleteAsync_PollNotFound_Throws()
    {
        _pollRepository
            .Setup(x => x.GetByIdAsync(99))
            .ReturnsAsync((Poll?)null);
        var service = CreateService();

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.DeleteAsync(99));

        Assert.Contains("not found", ex.Message);
    }
}