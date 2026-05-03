using FootballAI.Application.Interfaces.AuthInterfaces;
using FootballAI.Domain.Constants;
using FootballAI.Domain.Entities;
using FootballAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FootballAI.Infrastructure.Identity;

// ============================================================
// DATABASE SEEDER - Creates default roles and admin user
// ============================================================
public static class AuthSeeder
{
    public static async Task SeedAsync(
        AppDbContext db, IPasswordHasher hasher, IConfiguration config)
    {
        // Seed default roles
        foreach (var roleName in RoleNames.All)
        {
            if (!await db.Set<Role>().AnyAsync(r => r.Name == roleName))
            {
                db.Set<Role>().Add(new Role
                {
                    Name = roleName,
                    Description = $"{roleName} role"
                });
            }
        }
        await db.SaveChangesAsync();

        // Seed default admin user
        var adminEmail = config["Seeding:AdminEmail"];
        var adminPassword = config["Seeding:AdminPassword"];

        if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
            return;

        if (await db.Users.AnyAsync(u => u.Email == adminEmail.ToLower())) return;

        var (hash, salt) = hasher.HashPassword(adminPassword);
        var admin = new User
        {
            Email = adminEmail.ToLower(),
            Username = "admin",
            FirstName = "System",
            LastName = "Administrator",
            PasswordHash = hash,
            PasswordSalt = salt,
            EmailConfirmed = true,
            IsActive = true
        };

        var adminRole = await db.Set<Role>().FirstAsync(r => r.Name == RoleNames.Admin);
        admin.Roles.Add(new UserRole { RoleId = adminRole.Id });

        db.Users.Add(admin);
        await db.SaveChangesAsync();
    }
}

