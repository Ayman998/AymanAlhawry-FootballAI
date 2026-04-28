using FootballAI.Application.DTOs;
using FootballAI.src.FootballAI.Domain.Entities;

namespace FootballAI.src.FootballAI.Application.Interfaces;

public interface IPlayerTrackingService
{
    PlayerStats ComputePlayerStats(
        IEnumerable<DetectedPlayerDto> trackHistory,
        double fps,
        double metersPerPixel);
}
