using FootballAI.Application.DTOs;
using FootballAI.Application.Interfaces;

namespace FootballAI.src.FootballAI.ML.Trackers;

public class BallTracker : IBallTrackingService
{
    public Task<BallPositionDto?> TrackBallAsync(string framePath, CancellationToken ct = default)
    {
        // TODO: Implement ball tracking using computer vision
        return Task.FromResult<BallPositionDto?>(null);
    }
}
