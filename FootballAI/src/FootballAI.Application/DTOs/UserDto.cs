namespace FootballAI.Application.DTOs;

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public bool EmailConfirmed { get; set; }
    public List<string> Roles { get; set; } = new();
    public Guid? TenantId { get; set; }
    public DateTime? LastLoginAt { get; set; }
}
