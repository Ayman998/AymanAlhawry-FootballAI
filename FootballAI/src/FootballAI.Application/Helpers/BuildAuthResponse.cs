using FootballAI.Application.DTOs;
using FootballAI.Domain.Entities;

namespace FootballAI.Application.Services;

// Partial class split for AuthService - keeps BuildAuthResponse logic separate
public partial class AuthService
{
    private async Task<AuthResponseDto> BuildAuthResponse(
        User user, string ipAddress, CancellationToken ct)
    {
        var roles = user.Roles
            .Select(ur => ur.Role?.Name ?? string.Empty)
            .Where(n => !string.IsNullOrEmpty(n))
            .ToList();

        var accessToken = _jwtService.GenerateAccessToken(user, roles);

        var refreshToken = new RefreshToken
        {
            Token = _jwtService.GenerateRefreshToken(),
            ExpiresAt = _jwtService.GetRefreshTokenExpiry(),
            CreatedByIp = ipAddress,
            UserId = user.Id
        };
        _db.Set<RefreshToken>().Add(refreshToken);
        await _db.SaveChangesAsync(ct);

        return new AuthResponseDto
        {
            Success = true,
            Message = "Authentication successful",
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            AccessTokenExpiresAt = _jwtService.GetAccessTokenExpiry(),
            User = MapToUserDto(user, roles)
        };
    }
}
