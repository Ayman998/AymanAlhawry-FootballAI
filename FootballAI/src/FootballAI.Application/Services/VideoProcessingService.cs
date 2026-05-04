using FootballAI.Application.DTOs;
using FootballAI.Application.Interfaces;
using FootballAI.Domain.Entities;
using FootballAI.Domain.Enums;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FootballAI.Application.Services;

public class VideoProcessingService : IVideoProcessingService
{
    private readonly IBlobStorageService _blobStorage;
    private readonly IUnitOfWork _uow;
    private readonly IBackgroundJobClient _jobs;
    private readonly ILogger<VideoProcessingService> _logger;


    public VideoProcessingService(
        IBlobStorageService blobStorage,
        IUnitOfWork uow,
        IBackgroundJobClient jobs,
        ILogger<VideoProcessingService> logger)
    {
        _blobStorage = blobStorage;
        _uow = uow;
        _jobs = jobs;
        _logger = logger;
    }

    public async Task<VideoUploadResponseDto> ProcessVideoAsync(
        IFormFile file,
        VideoUploadDto metadata,
        CancellationToken ct = default)
    {
        // 1. Validate input
        if (file is null || file.Length == 0)
            throw new ArgumentException("Video file is empty");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var validExtensions = new[] { ".mp4", ".avi", ".mov", ".mkv" };
        if (!validExtensions.Contains(extension))
            throw new ArgumentException($"Unsupported video format: {extension}");

        // 2. Create VideoAnalysis record
        var videoAnalysis = new VideoAnalysis
        {
            OriginalFileName = file.FileName,
            FileSizeBytes = file.Length,
            Status = AnalysisStatus.Pending,
            CurrentStage = "Uploading"
        };
        var blobName = $"videos/{videoAnalysis.Id}/{extension}";
        videoAnalysis.BlobStorageUrl = await _blobStorage.UploadAsync(file, blobName, ct);

        await _uow.VideoAnalyses.AddAsync(videoAnalysis, ct);
        await _uow.SaveChangesAsync(ct);

        // 4. Queue background processing job
        var jobId = _jobs.Enqueue<IVideoAnalysisJob>(
            job => job.AnalyzeAsync(videoAnalysis.Id, metadata, CancellationToken.None));

        _logger.LogInformation(
        "Video {VideoId} queued for analysis. Job ID: {JobId}",
        videoAnalysis.Id, jobId);

        return new VideoUploadResponseDto
        {
            VideoId = videoAnalysis.Id,
            JobId = jobId,
            Status = "Queued",
            Message = "Your video has been queued for analysis."
        };

    }

    public async Task<AnalysisProgressDto> GetProgressAsync(Guid videoId, CancellationToken ct = default)
    {
        var analysis = await _uow.VideoAnalyses.GetByIdAsync(videoId, ct)
          ?? throw new KeyNotFoundException($"Video {videoId} not found");


        return new AnalysisProgressDto
        {
            VideoId = analysis.Id,
            Status = analysis.Status,
            ProgressPercent = analysis.ProgressPercent,
            CurrentStage = analysis.CurrentStage,
            ErrorMessage = analysis.ErrorMessage
        };
 
    }
}
