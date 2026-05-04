using FootballAI.Application.DTOs;
using FootballAI.Application.Models;

namespace FootballAI.Application.Helpers;

public static class PlayerTrackHelper
{
    public static List<PlayerTrack> GroupIntoPlayerTracks(List<DetectedPlayerDto> detections)
    {
        return detections
            .GroupBy(d => d.TeamColor)
            .Select(g => new PlayerTrack
            {
                TrackId = Guid.NewGuid(),
                TeamColor = g.Key,
                Detections = g.OrderBy(d => d.FrameIndex).ToList()
            })
            .ToList();
    }
}
