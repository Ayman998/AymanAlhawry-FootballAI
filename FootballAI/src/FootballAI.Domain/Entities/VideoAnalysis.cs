using FootballAI.Domain.Common;
using FootballAI.Domain.Enums;

namespace FootballAI.Domain.Entities;

public class VideoAnalysis : BaseEntity
{
    public string OriginalFileName { get; set; } = string.Empty;
    public string BlobStorageUrl { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public TimeSpan VideoDuration { get; set; }
    public int FramesPerSecond { get; set; }
    public int TotalFramesProcessed { get; set; }

    public AnalysisStatus Status { get; set; } = AnalysisStatus.Pending;
    public int ProgressPercent { get; set; }
    public string CurrentStage { get; set; } = string.Empty;

    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }

    public Guid? MatchId { get; set; }
    public Match? Match { get; set; }
}
