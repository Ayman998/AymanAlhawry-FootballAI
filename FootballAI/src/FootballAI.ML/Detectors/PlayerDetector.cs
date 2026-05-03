using FootballAI.Application.DTOs;
using FootballAI.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace FootballAI.src.FootballAI.ML.Detectors;

public class PlayerDetector : IPlayerDetectionService
{
    private readonly string _modelPath;
    private readonly ILogger<PlayerDetector> _logger;

    public PlayerDetector(string modelPath, ILogger<PlayerDetector> logger)
    {
        _modelPath = modelPath;
        _logger = logger;
    }

    public Task<List<DetectedPlayerDto>> DetectPlayersAsync(string framePath, CancellationToken ct = default)
    {
        _logger.LogDebug("Detecting players in frame {FramePath} using model {ModelPath}", framePath, _modelPath);
        // TODO: Implement YOLO-based player detection using ONNX Runtime
        return Task.FromResult(new List<DetectedPlayerDto>());
    }
}
