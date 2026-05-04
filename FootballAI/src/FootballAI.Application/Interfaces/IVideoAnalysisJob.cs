using FootballAI.Application.DTOs;

namespace FootballAI.Application.Interfaces;

public interface IVideoAnalysisJob
{
    Task AnalyzeAsync(Guid videoId, VideoUploadDto metadata, CancellationToken ct);
}
