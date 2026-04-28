using FootballAI.Application.DTOs;

namespace FootballAI.Application.Interfaces;

public interface IPlayerDetectionService
{
    Task<List<DetectedPlayerDto>> DetectPlayersAsync(string framePath, CancellationToken ct = default);

}
