using System;
using System.Collections.Generic;
using System.Text;

using Pollynx.Domain.Enums;

namespace Pollynx.Domain.Entities;

public class User
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<Vote> Votes { get; set; } = new List<Vote>();

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
