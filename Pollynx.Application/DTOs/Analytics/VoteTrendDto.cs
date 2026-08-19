using System;
using System.Collections.Generic;
using System.Text;

namespace Pollynx.Application.DTOs.Analytics;

public class VoteTrendDto
{
    public DateTime Date { get; set; }

    public int VoteCount { get; set; }
}
