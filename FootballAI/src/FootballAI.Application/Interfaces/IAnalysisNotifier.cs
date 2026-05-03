using FootballAI.Application.DTOs;

namespace FootballAI.Application.Interfaces;

public interface IAnalysisNotifier
{
    Task SendProgressAsync(Guid videoId, AnalysisProgressDto progress);
    Task SendCompletedAsync(Guid videoId, Guid matchId);
    Task SendFailedAsync(Guid videoId, string error);
}
