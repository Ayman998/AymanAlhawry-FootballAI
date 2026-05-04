using FootballAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FootballAI.Infrastructure.Data.Configurations;

public class PlayerStatsConfiguration : IEntityTypeConfiguration<PlayerStats>
{
    public void Configure(EntityTypeBuilder<PlayerStats> builder)
    {
        builder.ToTable("PlayerStats");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.HeatmapDataJson).HasColumnType("nvarchar(max)");
        builder.Ignore(s => s.PassAccuracy);
    }

}
