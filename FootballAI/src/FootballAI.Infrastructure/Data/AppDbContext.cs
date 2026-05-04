using FootballAI.Application.Interfaces;
using FootballAI.Domain.Common;
using FootballAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FootballAI.Infrastructure.Data;

public class AppDbContext : DbContext
{

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<MatchEvent> MatchEvents => Set<MatchEvent>();
    public DbSet<PlayerStats> PlayerStats => Set<PlayerStats>();
    public DbSet<VideoAnalysis> VideoAnalyses => Set<VideoAnalysis>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Soft delete query filter
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(
                    System.Linq.Expressions.Expression.Lambda(
                        System.Linq.Expressions.Expression.Equal(
                            System.Linq.Expressions.Expression.Property(
                                System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e"),
                                nameof(BaseEntity.IsDeleted)),
                            System.Linq.Expressions.Expression.Constant(false)),
                        System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e")));
            }
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
        return base.SaveChangesAsync(ct);
    }
}
