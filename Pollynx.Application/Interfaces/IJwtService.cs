using System;
using System.Collections.Generic;
using System.Text;

using Pollynx.Domain.Entities;

namespace Pollynx.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user);

    string GenerateRefreshToken();
}
