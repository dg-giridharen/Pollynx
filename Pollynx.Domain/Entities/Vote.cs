using System;
using System.Collections.Generic;
using System.Text;

namespace Pollynx.Domain.Entities;

public class Vote
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int PollId { get; set; }

    public int PollOptionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;

    public Poll Poll { get; set; } = null!;

    public PollOption PollOption { get; set; } = null!;
}
