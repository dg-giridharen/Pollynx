using System;
using System.Collections.Generic;
using System.Text;

namespace Pollynx.Application.DTOs.Analytics;

public class PollOptionResultDto
{
    public int PollOptionId { get; set; }

    public string OptionText { get; set; } = string.Empty;

    public int VoteCount { get; set; }

    public double Percentage { get; set; }
}
