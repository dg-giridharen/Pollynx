using System;
using System.Collections.Generic;
using System.Text;

using AutoMapper;
using Pollynx.Application.DTOs.Polls;
using Pollynx.Domain.Entities;

namespace Pollynx.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<PollOption, PollOptionResponseDto>();

        CreateMap<Poll, PollResponseDto>();
    }
}
