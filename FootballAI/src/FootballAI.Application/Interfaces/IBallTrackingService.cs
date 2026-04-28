using FootballAI.Application.DTOs;

namespace FootballAI.Application.Interfaces;

public interface IBallTrackingService
{
    Task<BallPositionDto?> TrackBallAsync(string framePath, CancellationToken ct = default);
}
