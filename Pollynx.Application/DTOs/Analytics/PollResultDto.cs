using System;
using System.Collections.Generic;
using System.Text;

namespace Pollynx.Application.DTOs.Analytics;

public class PollResultDto
{
    public int PollId { get; set; }

    public string PollTitle { get; set; } = string.Empty;

    public int TotalVotes { get; set; }

    public List<PollOptionResultDto> Options { get; set; } = new();
}
