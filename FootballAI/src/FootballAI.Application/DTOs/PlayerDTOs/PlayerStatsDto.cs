namespace FootballAI.Application.DTOs;

public class PlayerStatsDto
{
    public Guid PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int JerseyNumber { get; set; }
    public string Position { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;

    public double TotalDistanceKm { get; set; }
    public double MaxSpeedKmh { get; set; }
    public double AverageSpeedKmh { get; set; }
    public int SprintCount { get; set; }

    public int Touches { get; set; }
    public int PassesAttempted { get; set; }
    public int PassesCompleted { get; set; }
    public double PassAccuracy { get; set; }
    public int Goals { get; set; }
    public int Assists { get; set; }
    public int Shots { get; set; }
    public int Tackles { get; set; }

    public TimeSpan TimeOnPitch { get; set; }
    public List<HeatmapPointDto> HeatmapPoints { get; set; } = new();
}


