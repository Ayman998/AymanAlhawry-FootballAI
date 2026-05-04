using FootballAI.Domain.Enums;

namespace FootballAI.Application.DTOs.MatchDTOs;

public class MatchAnalysisDto
{
    public Guid MatchId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime MatchDate { get; set; }
    public string HomeTeam { get; set; } = string.Empty;
    public string AwayTeam { get; set; } = string.Empty;
    public int HomeScore { get; set; }
    public int AwayScore { get; set; }
    public AnalysisStatus Status { get; set; }
    public int ProgressPercent { get; set; }
    public List<MatchEventDto> Events { get; set; } = new();
    public TeamStatsDto HomeTeamStats { get; set; } = new();
    public TeamStatsDto AwayTeamStats { get; set; } = new();
}


