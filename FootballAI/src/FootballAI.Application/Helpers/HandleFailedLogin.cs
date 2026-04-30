using FootballAI.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FootballAI.Application.Services;

// Partial class split for AuthService - keeps HandleFailedLogin logic separate
public partial class AuthService
{
    private async Task HandleFailedLogin(User user, CancellationToken ct)
    {
        user.FailedLoginAttempts++;
        if (user.FailedLoginAttempts >= MaxFailedLoginAttempts)
        {
            user.LockoutEnd = DateTime.UtcNow.AddMinutes(LockoutMinutes);
            _logger.LogWarning("Account locked due to failed logins: {Email}", user.Email);
        }
        await _db.SaveChangesAsync(ct);
    }
}
