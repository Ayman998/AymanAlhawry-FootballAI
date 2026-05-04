using FootballAI.Domain.Enums;

namespace FootballAI.Application.DTOs;

public class AnalysisProgressDto
{
    public Guid VideoId { get; set; }
    public AnalysisStatus Status { get; set; }
    public int ProgressPercent { get; set; }
    public string CurrentStage { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}
