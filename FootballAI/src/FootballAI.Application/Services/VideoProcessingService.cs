using FootballAI.Application.DTOs;
using FootballAI.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FootballAI.Application.Services;

public class VideoProcessingService : IVideoProcessingService
{
    private readonly IBlobStorageService _blobStorage;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<VideoProcessingService> _logger;

    // In-memory progress store; replace with Redis/DB in production
    private static readonly Dictionary<Guid, AnalysisProgressDto> _progressStore = new();

    public VideoProcessingService(
        IBlobStorageService blobStorage,
        IUnitOfWork uow,
        ILogger<VideoProcessingService> logger)
    {
        _blobStorage = blobStorage;
        _uow = uow;
        _logger = logger;
    }

    public async Task<VideoUploadResponseDto> ProcessVideoAsync(
        IFormFile file,
        VideoUploadDto metadata,
        CancellationToken ct = default)
    {
        var videoId = Guid.NewGuid();
        var blobName = $"videos/{videoId}/{file.FileName}";

        _logger.LogInformation("Uploading video {FileName} as blob {BlobName}", file.FileName, blobName);

        var blobUrl = await _blobStorage.UploadAsync(file, blobName, ct);

        var progress = new AnalysisProgressDto
        {
            VideoId = videoId,
            Status = Domain.Enums.AnalysisStatus.Queued,
            ProgressPercent = 0,
            CurrentStage = "Uploaded"
        };
        _progressStore[videoId] = progress;

        return new VideoUploadResponseDto
        {
            VideoId = videoId,
            JobId = Guid.NewGuid().ToString(),
            Status = "Queued",
            Message = $"Video uploaded successfully. Processing will begin shortly. Blob: {blobUrl}"
        };
    }

    public Task<AnalysisProgressDto> GetProgressAsync(Guid videoId, CancellationToken ct = default)
    {
        if (!_progressStore.TryGetValue(videoId, out var progress))
            throw new KeyNotFoundException($"No video found with ID {videoId}");

        return Task.FromResult(progress);
    }
}
