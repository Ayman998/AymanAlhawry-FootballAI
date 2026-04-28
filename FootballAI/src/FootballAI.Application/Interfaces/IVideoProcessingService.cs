using FootballAI.Application.DTOs;
using FootballAI.src.FootballAI.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace FootballAI.src.FootballAI.Application.Interfaces;

public interface IVideoProcessingService
{
    Task<VideoUploadResponseDto> ProcessVideoAsync(
    IFormFile file,
    VideoUploadDto metadata,
    CancellationToken ct = default);

    Task<AnalysisProgressDto> GetProgressAsync(Guid videoId, CancellationToken ct = default);
}
