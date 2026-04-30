using FootballAI.Application.DTOs;
using FootballAI.Application.Interfaces;
using FootballAI.Domain.Entities;

namespace FootballAI.Application.Services;

public class PlayerTrackingService : IPlayerTrackingService
{
    public PlayerStats ComputePlayerStats(
        IEnumerable<DetectedPlayerDto> trackHistory,
        double fps,
        double metersPerPixel)
    {
        var history = trackHistory.OrderBy(h => h.FrameIndex).ToList();

        double totalDistancePixels = 0;
        double maxSpeedPixelsPerFrame = 0;

        for (int i = 1; i < history.Count; i++)
        {
            var prev = history[i - 1];
            var curr = history[i];
            double dx = curr.X - prev.X;
            double dy = curr.Y - prev.Y;
            double dist = Math.Sqrt(dx * dx + dy * dy);
            totalDistancePixels += dist;

            double framesElapsed = curr.FrameIndex - prev.FrameIndex;
            if (framesElapsed > 0)
            {
                double speedPixelsPerFrame = dist / framesElapsed;
                if (speedPixelsPerFrame > maxSpeedPixelsPerFrame)
                    maxSpeedPixelsPerFrame = speedPixelsPerFrame;
            }
        }

        double totalDistanceM = totalDistancePixels * metersPerPixel;
        double totalDistanceKm = totalDistanceM / 1000.0;
        double durationSeconds = history.Count > 0 ? history.Last().FrameIndex / fps : 0;
        double avgSpeedMs = durationSeconds > 0 ? totalDistanceM / durationSeconds : 0;
        double maxSpeedMs = maxSpeedPixelsPerFrame * fps * metersPerPixel;

        return new PlayerStats
        {
            TotalDistanceKm = Math.Round(totalDistanceKm, 3),
            MaxSpeedKmh = Math.Round(maxSpeedMs * 3.6, 2),
            AverageSpeedKmh = Math.Round(avgSpeedMs * 3.6, 2),
            TimeOnPitch = TimeSpan.FromSeconds(durationSeconds)
        };
    }
}
