using System.ComponentModel.DataAnnotations;

namespace FootballAI.Application.DTOs;

public class LoginRequestDto
{
    [Required]
    public string EmailOrUsername { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; } = false;
}
