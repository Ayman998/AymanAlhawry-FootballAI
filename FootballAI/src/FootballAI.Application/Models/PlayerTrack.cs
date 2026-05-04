using FootballAI.Application.DTOs;

namespace FootballAI.Application.Models;

public class PlayerTrack
{
    public Guid TrackId { get; set; }
    public Guid? PlayerId { get; set; }
    public string TeamColor { get; set; } = string.Empty;
    public List<DetectedPlayerDto> Detections { get; set; } = new();
}
