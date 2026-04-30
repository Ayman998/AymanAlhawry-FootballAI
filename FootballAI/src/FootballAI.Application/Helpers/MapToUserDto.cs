using FootballAI.Application.DTOs;
using FootballAI.Domain.Entities;

namespace FootballAI.Application.Services;

// Partial class split for AuthService - keeps MapToUserDto logic separate
public partial class AuthService
{
    private static UserDto MapToUserDto(User user, List<string> roles) => new()
    {
        Id = user.Id,
        Email = user.Email,
        Username = user.Username,
        FirstName = user.FirstName,
        LastName = user.LastName,
        EmailConfirmed = user.EmailConfirmed,
        Roles = roles,
        TenantId = user.TenantId,
        LastLoginAt = user.LastLoginAt
    };
}
