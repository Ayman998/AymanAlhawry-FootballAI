using FootballAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FootballAI.Application.Interfaces;

/// <summary>
/// Abstraction over the database context so Application services
/// do not depend directly on the Infrastructure project.
/// </summary>
public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<T> Set<T>() where T : class;
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
