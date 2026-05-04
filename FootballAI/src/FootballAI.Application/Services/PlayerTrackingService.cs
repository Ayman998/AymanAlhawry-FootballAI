using FootballAI.Application.DTOs;
using FootballAI.Application.Interfaces;
using FootballAI.Domain.Entities;
using System.Text.Json;

namespace FootballAI.Application.Services;

public class PlayerTrackingService : IPlayerTrackingService
{
    private const double SprintThresholdKmh = 25.0;
    private const double MinSprintDurationSeconds = 1.0;

    public PlayerStats ComputePlayerStats(
        IEnumerable<DetectedPlayerDto> trackHistory,
        double fps,
        double metersPerPixel)
    {
        var sortedTrack = trackHistory.OrderBy(p => p.FrameIndex).ToList();

        if (sortedTrack.Count < 2)
            return new PlayerStats { TotalDistanceKm = 0, MaxSpeedKmh = 0 };


        double totalDistanceMeters = 0;
        double maxSpeedKmh = 0;
        var speeds = new List<double>();

        for (int i = 1; i < sortedTrack.Count; i++)
        {
            var prev = sortedTrack[i - 1];
            var curr = sortedTrack[i];

            // Pixel distance between centers
            var prevCx = prev.X + prev.Width / 2;
            var prevCy = prev.Y + prev.Height / 2;
            var currCx = curr.X + curr.Width / 2;
            var currCy = curr.Y + curr.Height / 2;

            var pixelDistance = Math.Sqrt(
                Math.Pow(currCx - prevCx, 2) +
                Math.Pow(currCy - prevCy, 2));

            var meters = pixelDistance * metersPerPixel;
            var seconds = (curr.FrameIndex - prev.FrameIndex) / fps;
            var speedKmh = seconds > 0 ? (meters / seconds) * 3.6 : 0;

            // Filter unrealistic speeds (camera jumps, detection errors)
            if (speedKmh > 45) continue;

            totalDistanceMeters += meters;
            maxSpeedKmh = Math.Max(maxSpeedKmh, speedKmh);
            speeds.Add(speedKmh);
        }

        var avgSpeed = speeds.Count > 0 ? speeds.Average() : 0;
        var sprintCount = CountSprints(sortedTrack, fps, metersPerPixel);

        // Build heatmap data
        var heatmapPoints = sortedTrack
            .Select(p => new { X = p.X + p.Width / 2, Y = p.Y + p.Height / 2 })
            .ToList();

        return new PlayerStats
        {
            TotalDistanceKm = totalDistanceMeters / 1000,
            MaxSpeedKmh = Math.Round(maxSpeedKmh, 2),
            AverageSpeedKmh = Math.Round(avgSpeed, 2),
            SprintCount = sprintCount,
            HeatmapDataJson = JsonSerializer.Serialize(heatmapPoints)
        };

    }

    private int CountSprints(List<DetectedPlayerDto> track, double fps, double mpp)
    {
        var sprintCount = 0;
        var inSprint = false;
        var sprintFrames = 0;
        var minFrames = (int)(MinSprintDurationSeconds * fps);

        for (int i = 1; i < track.Count; i++)
        {
            var pixelDist = Math.Sqrt(
                Math.Pow(track[i].X - track[i - 1].X, 2) +
                Math.Pow(track[i].Y - track[i - 1].Y, 2));
            var seconds = (track[i].FrameIndex - track[i - 1].FrameIndex) / fps;
            var speedKmh = seconds > 0 ? (pixelDist * mpp / seconds) * 3.6 : 0;

            if (speedKmh >= SprintThresholdKmh)
            {
                sprintFrames++;
                if (!inSprint && sprintFrames >= minFrames)
                {
                    sprintCount++;
                    inSprint = true;
                }
            }
            else
            {
                inSprint = false;
                sprintFrames = 0;
            }
        }
        return sprintCount;
    }
}
