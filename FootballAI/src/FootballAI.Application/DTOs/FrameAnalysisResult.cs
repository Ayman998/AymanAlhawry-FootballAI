namespace FootballAI.Application.DTOs;

public class FrameAnalysisResult
{
    public int FrameIndex { get; set; }
    public TimeSpan Timestamp { get; set; }
    public List<DetectedPlayerDto> Players { get; set; } = new();
    public BallPositionDto? Ball { get; set; }
}
