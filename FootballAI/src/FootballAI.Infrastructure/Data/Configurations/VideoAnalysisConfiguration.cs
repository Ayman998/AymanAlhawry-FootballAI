using FootballAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FootballAI.Infrastructure.Data.Configurations;

public class VideoAnalysisConfiguration : IEntityTypeConfiguration<VideoAnalysis>
{
    public void Configure(EntityTypeBuilder<VideoAnalysis> builder)
    {
        builder.ToTable("VideoAnalyses");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.OriginalFileName).IsRequired().HasMaxLength(300);
        builder.Property(v => v.BlobStorageUrl).HasMaxLength(1000);
        builder.Property(v => v.CurrentStage).HasMaxLength(100);
        builder.Property(v => v.ErrorMessage).HasMaxLength(2000);
    }
}
