using FootballAI.Domain.Common;

namespace FootballAI.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";

    // Hashed password - NEVER stored in plain text
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;

    public bool EmailConfirmed { get; set; } = false;
    public string? EmailConfirmationToken { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LockoutEnd { get; set; }

    // Multi-tenancy support - which client organization the user belongs to
    public Guid? TenantId { get; set; }

    // Roles (e.g., Admin, Coach, Analyst, Viewer)
    public ICollection<UserRole> Roles { get; set; } = new List<UserRole>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
