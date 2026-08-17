using System;
using System.Collections.Generic;
using System.Text;

namespace Pollynx.Domain.Entities;

public class PollOption
{
    public int Id { get; set; }

    public int PollId { get; set; }

    public string OptionText { get; set; } = string.Empty;

    public Poll Poll { get; set; } = null!;

    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
}
