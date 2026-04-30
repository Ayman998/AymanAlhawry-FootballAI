using FootballAI.Application.DTOs;
using FootballAI.Application.Interfaces;
using FootballAI.Application.Interfaces.AuthInterfaces;
using FootballAI.Domain.Constants;
using FootballAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace FootballAI.Application.Services;

public partial class AuthService : IAuthService
{
    private readonly IAppDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtService;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthService> _logger;

    private const int MaxFailedLoginAttempts = 5;
    private const int LockoutMinutes = 15;

    public AuthService(
        IAppDbContext db,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtService,
        IEmailService emailService,
        ILogger<AuthService> logger)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<bool> AssignRoleAsync(Guid userId, string roleName, CancellationToken ct = default)
    {
        var user = await _db.Users.Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null) return false;

        var role = await _db.Set<Role>().FirstOrDefaultAsync(r => r.Name == roleName, ct);
        if (role is null) return false;

        if (user.Roles.Any(ur => ur.RoleId == role.Id)) return true;

        user.Roles.Add(new UserRole { UserId = userId, RoleId = role.Id });
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request, CancellationToken ct = default)
    {
        var user = await _db.Users.FindAsync(new object[] { userId }, ct);
        if (user is null) return false;

        if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash, user.PasswordSalt))
            return false;

        var (hash, salt) = _passwordHasher.HashPassword(request.NewPassword);
        user.PasswordHash = hash;
        user.PasswordSalt = salt;

        var activeTokens = await _db.Set<RefreshToken>()
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ToListAsync(ct);
        foreach (var token in activeTokens)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedReason = "Password changed";
        }

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Password changed for user {UserId}", userId);
        return true;
    }

    public async Task<bool> ConfirmEmailAsync(string email, string token, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email.ToLower(), ct);
        if (user is null) return false;

        if (user.EmailConfirmationToken != token) return false;

        user.EmailConfirmed = true;
        user.EmailConfirmationToken = null;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<UserDto?> GetCurrentUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _db.Users
            .Include(u => u.Roles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user is null) return null;
        var roles = user.Roles.Select(ur => ur.Role.Name).ToList();
        return MapToUserDto(user, roles);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, string ipAddress, CancellationToken ct = default)
    {
        var input = request.EmailOrUsername.Trim().ToLower();

        var user = await _db.Users
            .Include(u => u.Roles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == input || u.Username == request.EmailOrUsername, ct);

        if (user is null)
            return Fail("Invalid credentials");

        if (user.LockoutEnd is not null && user.LockoutEnd > DateTime.UtcNow)
        {
            var minutesLeft = Math.Ceiling((user.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes);
            return Fail($"Account locked. Try again in {minutesLeft} minute(s).");
        }

        if (!user.IsActive)
            return Fail("Account is disabled. Please contact support.");

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
        {
            await HandleFailedLogin(user, ct);
            return Fail("Invalid credentials");
        }

        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;
        user.LastLoginAt = DateTime.UtcNow;

        var response = await BuildAuthResponse(user, ipAddress, ct);
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("User logged in: {Email}", user.Email);

        return response;
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, string ipAddress, CancellationToken ct = default)
    {
        var existingToken = await _db.Set<RefreshToken>()
            .Include(rt => rt.User).ThenInclude(u => u.Roles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken, ct);

        if (existingToken is null || !existingToken.IsActive)
            return Fail("Invalid or expired refresh token");

        existingToken.RevokedAt = DateTime.UtcNow;
        existingToken.RevokedReason = "Replaced by new token";
        existingToken.ReplacedByToken = _jwtService.GenerateRefreshToken();

        var newRefreshToken = new RefreshToken
        {
            Token = existingToken.ReplacedByToken,
            ExpiresAt = _jwtService.GetRefreshTokenExpiry(),
            CreatedByIp = ipAddress,
            UserId = existingToken.UserId
        };
        _db.Set<RefreshToken>().Add(newRefreshToken);

        var roles = existingToken.User.Roles.Select(ur => ur.Role.Name).ToList();
        var accessToken = _jwtService.GenerateAccessToken(existingToken.User, roles);

        await _db.SaveChangesAsync(ct);

        return new AuthResponseDto
        {
            Success = true,
            Message = "Token refreshed",
            AccessToken = accessToken,
            RefreshToken = newRefreshToken.Token,
            AccessTokenExpiresAt = _jwtService.GetAccessTokenExpiry(),
            User = MapToUserDto(existingToken.User, roles)
        };
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, string ipAddress, CancellationToken ct = default)
    {
        if (await _db.Users.AnyAsync(u => u.Email == request.Email.ToLower(), ct))
            return Fail("An account with this email already exists");

        if (await _db.Users.AnyAsync(u => u.Username == request.Username, ct))
            return Fail("This username is already taken");

        var (hash, salt) = _passwordHasher.HashPassword(request.Password);

        var user = new User
        {
            Email = request.Email.ToLower(),
            Username = request.Username,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PasswordHash = hash,
            PasswordSalt = salt,
            EmailConfirmationToken = GenerateSecureToken(),
            TenantId = request.TenantId,
            IsActive = true
        };

        var viewerRole = await _db.Set<Role>().FirstOrDefaultAsync(r => r.Name == RoleNames.Viewer, ct);
        if (viewerRole is not null)
            user.Roles.Add(new UserRole { RoleId = viewerRole.Id });

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        try
        {
            await _emailService.SendEmailConfirmationAsync(user.Email, user.EmailConfirmationToken!, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send confirmation email to {Email}", user.Email);
        }

        _logger.LogInformation("User registered: {Email}", user.Email);
        return await BuildAuthResponse(user, ipAddress, ct);
    }

    public async Task<bool> RemoveRoleAsync(Guid userId, string roleName, CancellationToken ct = default)
    {
        var role = await _db.Set<Role>().FirstOrDefaultAsync(r => r.Name == roleName, ct);
        if (role is null) return false;

        var userRole = await _db.Set<UserRole>()
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == role.Id, ct);
        if (userRole is null) return false;

        _db.Set<UserRole>().Remove(userRole);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> RequestPasswordResetAsync(string email, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email.ToLower(), ct);
        if (user is null) return true; // Prevent email enumeration

        var resetToken = GenerateSecureToken();
        user.EmailConfirmationToken = resetToken;
        await _db.SaveChangesAsync(ct);

        try
        {
            await _emailService.SendPasswordResetAsync(user.Email, resetToken, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset to {Email}", user.Email);
        }
        return true;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email.ToLower(), ct);
        if (user is null) return false;

        if (user.EmailConfirmationToken != request.ResetToken) return false;

        var (hash, salt) = _passwordHasher.HashPassword(request.NewPassword);
        user.PasswordHash = hash;
        user.PasswordSalt = salt;
        user.EmailConfirmationToken = null;
        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Password reset for user {Email}", user.Email);
        return true;
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken, string ipAddress, CancellationToken ct = default)
    {
        var token = await _db.Set<RefreshToken>().FirstOrDefaultAsync(rt => rt.Token == refreshToken, ct);

        if (token is null || !token.IsActive) return false;

        token.RevokedAt = DateTime.UtcNow;
        token.RevokedReason = $"Revoked by user from {ipAddress}";

        await _db.SaveChangesAsync(ct);
        return true;
    }

    private static AuthResponseDto Fail(string message)
        => new() { Success = false, Message = message };

    private static string GenerateSecureToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }
}
