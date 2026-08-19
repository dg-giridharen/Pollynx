using System;
using System.Collections.Generic;
using System.Text;

using System.ComponentModel.DataAnnotations;

namespace Pollynx.Application.DTOs.Votes;

public class VoteRequestDto
{
    [Required]
    public int PollOptionId { get; set; }
}
