using FootballAI.Application.DTOs;
using FootballAI.Domain.Entities;

namespace FootballAI.Application.Interfaces;

public interface IPlayerTrackingService
{
    PlayerStats ComputePlayerStats(
        IEnumerable<DetectedPlayerDto> trackHistory,
        double fps,
        double metersPerPixel);
}
