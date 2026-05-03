using System.ComponentModel.DataAnnotations;

namespace FootballAI.Application.DTOs;

public class AssignRoleRequestDto
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public string RoleName { get; set; } = string.Empty;
}
