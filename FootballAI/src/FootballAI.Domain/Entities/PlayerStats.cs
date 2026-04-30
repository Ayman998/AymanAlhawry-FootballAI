using FootballAI.Domain.Common;

namespace FootballAI.Domain.Entities;

public class PlayerStats : BaseEntity
{
    public Guid MatchId { get; set; }
    public Match Match { get; set; } = null!;

    public Guid PlayerId { get; set; }
    public Player Player { get; set; } = null!;

    // Movement metrics
    public double TotalDistanceKm { get; set; }
    public double MaxSpeedKmh { get; set; }
    public double AverageSpeedKmh { get; set; }
    public int SprintCount { get; set; }
    public double SprintDistanceKm { get; set; }

    // Ball involvement
    public int Touches { get; set; }
    public int PassesAttempted { get; set; }
    public int PassesCompleted { get; set; }
    public double PassAccuracy => PassesAttempted == 0 ? 0
        : (double)PassesCompleted / PassesAttempted * 100;
    public int Shots { get; set; }
    public int ShotsOnTarget { get; set; }
    public int Goals { get; set; }
    public int Assists { get; set; }

    // Defensive
    public int Tackles { get; set; }
    public int Interceptions { get; set; }
    public int Clearances { get; set; }
    public int Fouls { get; set; }

    // Time on pitch
    public TimeSpan TimeOnPitch { get; set; }

    // Stored as JSON in DB - heatmap positions
    public string HeatmapDataJson { get; set; } = "[]";
}
