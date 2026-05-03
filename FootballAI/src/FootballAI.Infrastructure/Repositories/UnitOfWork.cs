using FootballAI.Application.Interfaces;
using FootballAI.Domain.Common;
using FootballAI.Domain.Entities;
using FootballAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FootballAI.src.FootballAI.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;

    public UnitOfWork(AppDbContext db)
    {
        _db = db;
        Matches = new GenericRepository<Match>(db);
        Players = new GenericRepository<Player>(db);
        Teams = new GenericRepository<Team>(db);
        MatchEvents = new GenericRepository<MatchEvent>(db);
        VideoAnalyses = new GenericRepository<VideoAnalysis>(db);
    }

    public IRepository<Match> Matches { get; }
    public IRepository<Player> Players { get; }
    public IRepository<Team> Teams { get; }
    public IRepository<MatchEvent> MatchEvents { get; }
    public IRepository<VideoAnalysis> VideoAnalyses { get; }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);

    private sealed class GenericRepository<T> : IRepository<T> where T : BaseEntity
    {
        private readonly DbSet<T> _set;

        public GenericRepository(AppDbContext db) => _set = db.Set<T>();

        public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _set.FindAsync(new object[] { id }, ct);

        public async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
            => await _set.ToListAsync(ct);

        public async Task AddAsync(T entity, CancellationToken ct = default)
            => await _set.AddAsync(entity, ct);

        public Task UpdateAsync(T entity, CancellationToken ct = default)
        {
            _set.Update(entity);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _set.FindAsync(new object[] { id }, ct);
            if (entity is not null)
                _set.Remove(entity);
        }
    }
}
