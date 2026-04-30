using FootballAI.Application.DTOs;

namespace FootballAI.Application.Interfaces.AuthInterfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, string ipAddress, CancellationToken ct = default);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request, string ipAddress, CancellationToken ct = default);
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, string ipAddress, CancellationToken ct = default);
    Task<bool> RevokeTokenAsync(string refreshToken, string ipAddress, CancellationToken ct = default);
    Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request, CancellationToken ct = default);
    Task<bool> ConfirmEmailAsync(string email, string token, CancellationToken ct = default);
    Task<bool> RequestPasswordResetAsync(string email, CancellationToken ct = default);
    Task<bool> ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken ct = default);
    Task<bool> AssignRoleAsync(Guid userId, string roleName, CancellationToken ct = default);
    Task<bool> RemoveRoleAsync(Guid userId, string roleName, CancellationToken ct = default);
    Task<UserDto?> GetCurrentUserAsync(Guid userId, CancellationToken ct = default);
}
