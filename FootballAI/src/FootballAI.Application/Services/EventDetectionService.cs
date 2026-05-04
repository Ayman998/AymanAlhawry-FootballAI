using FootballAI.Application.DTOs;
using FootballAI.Application.Interfaces;
using FootballAI.Domain.Entities;
using FootballAI.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace FootballAI.Application.Services;

public class EventDetectionService : IEventDetectionService
{
    private readonly ILogger<EventDetectionService> _logger;

    public EventDetectionService(ILogger<EventDetectionService> logger)
    {
        _logger = logger;
    }

    public Task<List<MatchEvent>> DetectEventsAsync(
        Guid matchId,
        IEnumerable<FrameAnalysisResult> frameResults,
        CancellationToken ct = default)
    {
        var events = new List<MatchEvent>();
        var frames = frameResults.OrderBy(f => f.FrameIndex).ToList();

        // Detect ball-related events through trajectory analysis
        events.AddRange(DetectShots(matchId, frames));
        events.AddRange(DetectPasses(matchId, frames));

        _logger.LogInformation("Detected {Count} events for match {MatchId}", events.Count, matchId);
        return Task.FromResult(events);
    }

    private List<MatchEvent> DetectShots(Guid matchId, List<FrameAnalysisResult> frames)
    {
        var shots = new List<MatchEvent>();
        const double goalAreaXMin = 1700;  // approx. goal x coord on right side
        const double shotSpeedThreshold = 60.0; // pixels per frame

        for (int i = 5; i < frames.Count; i++)
        {
            var current = frames[i].Ball;
            var previous = frames[i - 5].Ball;

            if (current is null || previous is null) continue;

            var dx = current.X - previous.X;
            var dy = current.Y - previous.Y;
            var velocity = Math.Sqrt(dx * dx + dy * dy);

            if (velocity > shotSpeedThreshold && current.X > goalAreaXMin)
            {
                shots.Add(new MatchEvent
                {
                    MatchId = matchId,
                    Type = EventType.Shot,
                    Timestamp = frames[i].Timestamp,
                    FieldPositionX = current.X,
                    FieldPositionY = current.Y,
                    ConfidenceScore = 0.75,
                    Description = "High-velocity ball movement towards goal area detected"
                });
            }
        }
        return shots;
    }

    private List<MatchEvent> DetectPasses(Guid matchId, List<FrameAnalysisResult> frames)
    {
        // Simplified pass detection: ball changes possession between same-team players
        var passes = new List<MatchEvent>();
        // Production implementation would use proximity + team color + ball trajectory
        return passes;
    }

}
