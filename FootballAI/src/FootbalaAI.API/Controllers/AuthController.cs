using FootballAI.Application.DTOs;
using FootballAI.Application.Interfaces.AuthInterfaces;
using FootballAI.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FootballAI.src.FootbalaAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    // ============================================================
    // POST /api/auth/register
    // ============================================================
    /// <summary>
    /// Registers a new user account and returns an authentication token.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponseDto>> Register(
        [FromBody] RegisterRequestDto request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var response = await _authService.RegisterAsync(request, GetClientIp(), ct);
            if (!response.Success) return BadRequest(response);

            SetRefreshTokenCookie(response.RefreshToken!);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed for {Email}", request.Email);
            return StatusCode(500, new AuthResponseDto
            {
                Success = false,
                Message = "An error occurred during registration"
            });
        }
    }

    // ============================================================
    // POST /api/auth/login
    // ============================================================
    /// <summary>
    /// Authenticates a user and returns an access token.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Login(
        [FromBody] LoginRequestDto request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var response = await _authService.LoginAsync(request, GetClientIp(), ct);
            if (!response.Success) return Unauthorized(response);

            SetRefreshTokenCookie(response.RefreshToken!);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed for {Input}", request.EmailOrUsername);
            return StatusCode(500, new AuthResponseDto
            {
                Success = false,
                Message = "An error occurred during login"
            });
        }
    }

    // ============================================================
    // POST /api/auth/refresh
    // ============================================================
    /// <summary>
    /// Refreshes an expired access token using a refresh token.
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Refresh(
        [FromBody] RefreshTokenRequestDto? request, CancellationToken ct)
    {
        // Try cookie first, then fall back to body
        var refreshToken = Request.Cookies["refreshToken"] ?? request?.RefreshToken;
        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized(new AuthResponseDto
            {
                Success = false,
                Message = "Refresh token is required"
            });

        var response = await _authService.RefreshTokenAsync(refreshToken, GetClientIp(), ct);
        if (!response.Success) return Unauthorized(response);

        SetRefreshTokenCookie(response.RefreshToken!);
        return Ok(response);
    }

    // ============================================================
    // POST /api/auth/logout
    // ============================================================
    /// <summary>
    /// Logs out the current user by revoking their refresh token.
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout(
        [FromBody] RefreshTokenRequestDto? request, CancellationToken ct)
    {
        var refreshToken = Request.Cookies["refreshToken"] ?? request?.RefreshToken;
        if (!string.IsNullOrEmpty(refreshToken))
            await _authService.RevokeTokenAsync(refreshToken, GetClientIp(), ct);

        Response.Cookies.Delete("refreshToken");
        return Ok(new { message = "Logged out successfully" });
    }

    // ============================================================
    // GET /api/auth/me
    // ============================================================
    /// <summary>
    /// Returns the currently authenticated user's profile.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserDto>> GetCurrentUser(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var user = await _authService.GetCurrentUserAsync(userId.Value, ct);
        return user is null ? NotFound() : Ok(user);
    }

    // ============================================================
    // POST /api/auth/change-password
    // ============================================================
    /// <summary>
    /// Changes the current user's password.
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequestDto request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var result = await _authService.ChangePasswordAsync(userId.Value, request, ct);
        return result
            ? Ok(new { message = "Password changed successfully. Please log in again." })
            : BadRequest(new { message = "Current password is incorrect" });
    }

    // ============================================================
    // POST /api/auth/forgot-password
    // ============================================================
    /// <summary>
    /// Sends a password reset email if the account exists.
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequestDto request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        await _authService.RequestPasswordResetAsync(request.Email, ct);
        // Always return success message to prevent enumeration
        return Ok(new { message = "If an account exists with that email, a reset link has been sent" });
    }

    // ============================================================
    // POST /api/auth/reset-password
    // ============================================================
    /// <summary>
    /// Resets a user's password using the token sent by email.
    /// </summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequestDto request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _authService.ResetPasswordAsync(request, ct);
        return result
            ? Ok(new { message = "Password has been reset. You can now log in." })
            : BadRequest(new { message = "Invalid reset token or email" });
    }

    // ============================================================
    // GET /api/auth/confirm-email
    // ============================================================
    /// <summary>
    /// Confirms a user's email address using the token from the confirmation link.
    /// </summary>
    [HttpGet("confirm-email")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmEmail(
        [FromQuery] string email,
        [FromQuery] string token,
        CancellationToken ct)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            return BadRequest(new { message = "Email and token are required" });

        var result = await _authService.ConfirmEmailAsync(email, token, ct);
        return result
            ? Ok(new { message = "Email confirmed successfully" })
            : BadRequest(new { message = "Invalid confirmation link" });
    }

    // ============================================================
    // POST /api/auth/assign-role  (Admin only)
    // ============================================================
    /// <summary>
    /// Assigns a role to a user. Requires Admin privileges.
    /// </summary>
    [HttpPost("assign-role")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AssignRole(
        [FromBody] AssignRoleRequestDto request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _authService.AssignRoleAsync(request.UserId, request.RoleName, ct);
        return result
            ? Ok(new { message = $"Role '{request.RoleName}' assigned" })
            : BadRequest(new { message = "Failed to assign role. Check user and role names." });
    }

    // ============================================================
    // DELETE /api/auth/remove-role  (Admin only)
    // ============================================================
    /// <summary>
    /// Removes a role from a user. Requires Admin privileges.
    /// </summary>
    [HttpDelete("remove-role")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveRole(
        [FromBody] AssignRoleRequestDto request, CancellationToken ct)
    {
        var result = await _authService.RemoveRoleAsync(request.UserId, request.RoleName, ct);
        return result
            ? Ok(new { message = $"Role '{request.RoleName}' removed" })
            : BadRequest(new { message = "Failed to remove role" });
    }

    // ============================================================
    // PRIVATE HELPERS
    // ============================================================
    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdClaim, out var id) ? id : null;
    }

    private string GetClientIp()
    {
        // Check forwarded headers first (when behind a reverse proxy)
        if (Request.Headers.TryGetValue("X-Forwarded-For", out var forwarded)
            && !string.IsNullOrEmpty(forwarded))
        {
            return forwarded.ToString().Split(',')[0].Trim();
        }
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private void SetRefreshTokenCookie(string token)
    {
        // HttpOnly cookie - inaccessible from JavaScript = protection from XSS
        Response.Cookies.Append("refreshToken", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,           // HTTPS only
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7),
            Path = "/api/auth"
        });
    }
}
