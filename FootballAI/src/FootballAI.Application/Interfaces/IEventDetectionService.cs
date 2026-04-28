using FootballAI.Application.DTOs;
using FootballAI.src.FootballAI.Domain.Entities;

namespace FootballAI.Application.Interfaces;

public interface IEventDetectionService
{
    Task<List<MatchEvent>> DetectEventsAsync(
    Guid matchId,
    IEnumerable<FrameAnalysisResult> frameResults,
    CancellationToken ct = default);
}
