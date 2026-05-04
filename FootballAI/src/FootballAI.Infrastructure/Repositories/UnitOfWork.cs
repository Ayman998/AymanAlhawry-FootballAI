using FootballAI.Application.Interfaces;
using FootballAI.Domain.Common;
using FootballAI.Domain.Entities;
using FootballAI.Infrastructure.Data;
using FootballAI.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FootballAI.src.FootballAI.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;
    public IRepository<Match> Matches { get; }
    public IRepository<Player> Players { get; }
    public IRepository<Team> Teams { get; }
    public IRepository<MatchEvent> MatchEvents { get; }
    public IRepository<VideoAnalysis> VideoAnalyses { get; } 

    public UnitOfWork(AppDbContext db)
    {
        _db = db;
        Matches = new Repository<Match>(db);
        Players = new Repository<Player>(db);
        Teams = new Repository<Team>(db);
        MatchEvents = new Repository<MatchEvent>(db);
        VideoAnalyses = new Repository<VideoAnalysis>(db);
    }



    public Task<int> SaveChangesAsync(CancellationToken ct = default)
       => _db.SaveChangesAsync(ct);

}
