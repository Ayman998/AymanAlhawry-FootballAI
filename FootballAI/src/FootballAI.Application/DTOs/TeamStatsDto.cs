namespace FootballAI.Application.DTOs;

public class TeamStatsDto
{
    public Guid TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;

    public double PossessionPercent { get; set; }
    public int TotalPasses { get; set; }
    public int CompletedPasses { get; set; }
    public double PassAccuracy { get; set; }
    public int Shots { get; set; }
    public int ShotsOnTarget { get; set; }
    public int Corners { get; set; }
    public int Fouls { get; set; }
    public string Formation { get; set; } = string.Empty;
    public double TotalDistanceKm { get; set; }
}
