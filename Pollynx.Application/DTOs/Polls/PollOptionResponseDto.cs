using System;
using System.Collections.Generic;
using System.Text;

namespace Pollynx.Application.DTOs.Polls;

public class PollOptionResponseDto
{
    public int Id { get; set; }

    public string OptionText { get; set; } = string.Empty;
}
