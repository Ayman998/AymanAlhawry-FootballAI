using FootballAI.src.FootballAI.Domain.Entities;

namespace FootballAI.Application.Interfaces;

public interface IUnitOfWork
{
    IRepository<Match> Matches { get; }
    IRepository<Player> Players { get; }
    IRepository<Team> Teams { get; }
    IRepository<MatchEvent> MatchEvents { get; }
    IRepository<VideoAnalysis> VideoAnalyses { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
