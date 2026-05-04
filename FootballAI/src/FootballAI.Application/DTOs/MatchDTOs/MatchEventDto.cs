using FootballAI.Domain.Enums;

namespace FootballAI.Application.DTOs;

public class MatchEventDto
{
    public Guid Id { get; set; }
    public EventType Type { get; set; }
    public TimeSpan Timestamp { get; set; }
    public TeamSide Team { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; }
    public string? VideoClipUrl { get; set; }
}
