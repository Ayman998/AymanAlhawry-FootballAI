using FootballAI.Application.Interfaces;
using FootballAI.Domain.Common;
using FootballAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FootballAI.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _set;

    public Repository(AppDbContext context)
    {
        _context = context;
        _set = context.Set<T>();
    }
    public async Task AddAsync(T entity, CancellationToken ct = default)
        => await _set.AddAsync(entity, ct);

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await GetByIdAsync(id, ct);
        if (entity is not null)
        {
            entity.IsDeleted = true;  // soft delete
            _set.Update(entity);
        }
    }

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
         => await _set.AsNoTracking().ToListAsync(ct);

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
      => await _set.FindAsync(new object[] { id }, ct);

    public Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        _set.Update(entity);
        return Task.CompletedTask;
    }
}
