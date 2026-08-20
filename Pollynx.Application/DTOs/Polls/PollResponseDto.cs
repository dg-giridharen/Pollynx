using System;
using System.Collections.Generic;
using System.Text;

namespace Pollynx.Application.DTOs.Polls;

public class PollResponseDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public bool IsPublic { get; set; }

    public bool IsClosed { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<PollOptionResponseDto> Options { get; set; } = new();
}
