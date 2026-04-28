using FootballAI.src.FootballAI.Application.DTOs;

namespace FootballAI.Application.Interfaces;

public interface ITeamAnalysisService
{
    Task<TeamStatsDto> AnalyzeTeamAsync(Guid matchId, Guid teamId, CancellationToken ct = default);
}
