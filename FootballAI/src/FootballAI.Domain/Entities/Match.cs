using FootballAI.src.FootballAI.Domain.Common;

namespace FootballAI.src.FootballAI.Domain.Entities;

public class Match : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public DateTime MatchDate { get; set; }
    public string Venue { get; set; } = string.Empty;
    public MatchType Type { get; set; }
    public TimeSpan Duration { get; set; }

    public Guid HomeTeamId { get; set; }
    public Team HomeTeam { get; set; } = null!;

    public Guid AwayTeamId { get; set; }
    public Team AwayTeam { get; set; } = null!;

    public int HomeScore { get; set; }
    public int AwayScore { get; set; }

    public Guid VideoAnalysisId { get; set; }
    public VideoAnalysis VideoAnalysis { get; set; } = null!;

    public ICollection<MatchEvent> Events { get; set; } = new List<MatchEvent>();
    public ICollection<PlayerStats> PlayerStats { get; set; } = new List<PlayerStats>();
}
