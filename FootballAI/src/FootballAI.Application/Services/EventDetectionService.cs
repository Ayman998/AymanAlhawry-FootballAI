using FootballAI.Application.DTOs;
using FootballAI.Application.Interfaces;
using FootballAI.Domain.Entities;
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
        _logger.LogInformation("Detecting events for match {MatchId}", matchId);

        // Placeholder: return empty event list
        // Replace with real ML-based event detection logic
        var events = new List<MatchEvent>();
        return Task.FromResult(events);
    }
}
