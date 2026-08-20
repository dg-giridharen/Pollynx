using Pollynx.Application.DTOs.Polls;

namespace Pollynx.Application.Interfaces;

public interface IPollService
{
    Task<PollResponseDto> CreateAsync(CreatePollDto request);

    Task<List<PollResponseDto>> GetAllAsync();

    Task<PollResponseDto> GetByIdAsync(int id);

    Task<PollResponseDto> UpdateAsync(
        int id,
        UpdatePollDto request);

    Task DeleteAsync(int id);

    Task CloseAsync(int id);

    Task<List<PollResponseDto>> GetActivePollsAsync();
}