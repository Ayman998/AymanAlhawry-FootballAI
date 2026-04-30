using FootballAI.Domain.Common;
using FootballAI.Domain.Enums;

namespace FootballAI.Domain.Entities;

public class MatchEvent : BaseEntity
{
    public Guid MatchId { get; set; }
    public Match Match { get; set; } = null!;

    public EventType Type { get; set; }
    public TimeSpan Timestamp { get; set; }
    public TeamSide Team { get; set; }

    public Guid? PlayerId { get; set; }
    public Player? Player { get; set; }

    public Guid? AssistPlayerId { get; set; }
    public Player? AssistPlayer { get; set; }

    public double FieldPositionX { get; set; }
    public double FieldPositionY { get; set; }

    public string Description { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; }

    public string? VideoClipUrl { get; set; }
}
