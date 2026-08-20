using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Pollynx.Domain.Entities;
using Pollynx.Domain.Enums;

namespace Pollynx.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext context)
    {
        await context.Database.MigrateAsync();

        if (await context.Users.AnyAsync())
        {
            return;
        }

        var admin = new User
        {
            FullName = "System Admin",
            Email = "admin@pollynx.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Role = UserRole.Admin,
            CreatedAt = DateTime.UtcNow
        };

        var user = new User
        {
            FullName = "Test User",
            Email = "user@pollynx.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123"),
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow
        };

        await context.Users.AddRangeAsync(admin, user);

        await context.SaveChangesAsync();
    }
}