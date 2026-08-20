using System;
using System.Collections.Generic;
using System.Text;

namespace Pollynx.Domain.Entities;

public class Poll
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public bool IsPublic { get; set; }

    public bool IsClosed { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<PollOption> Options { get; set; } = new List<PollOption>();

    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
}
