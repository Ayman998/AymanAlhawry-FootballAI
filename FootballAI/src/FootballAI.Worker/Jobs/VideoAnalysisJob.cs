using FootballAI.Application.DTOs;
using FootballAI.Application.Interfaces;
using FootballAI.Domain.Enums;
using FootballAI.src.FootballAI.ML.Video;
using FootballAI.Domain.Entities;
using static FootballAI.Application.Helpers.AnalysisStatusHelper;
using static FootballAI.Application.Helpers.PlayerTrackHelper;
using static FootballAI.Application.Helpers.FieldScaleComputer;

namespace FootballAI.src.FootballAI.Worker.Jobs;

public class VideoAnalysisJob : IVideoAnalysisJob
{
    private readonly IUnitOfWork _uow;
    private readonly IBlobStorageService _storage;
    private readonly FrameExtractor _frameExtractor;
    private readonly IPlayerDetectionService _playerDetector;
    private readonly IBallTrackingService _ballTracker;
    private readonly IPlayerTrackingService _playerTracker;
    private readonly IEventDetectionService _eventDetector;
    private readonly IHeatmapService _heatmapService;
    private readonly IAnalysisNotifier _notifier;
    private readonly ILogger<VideoAnalysisJob> _logger;

    public VideoAnalysisJob(
        IUnitOfWork uow,
        IBlobStorageService storage,
        FrameExtractor frameExtractor,
        IPlayerDetectionService playerDetector,
        IBallTrackingService ballTracker,
        IPlayerTrackingService playerTracker,
        IEventDetectionService eventDetector,
        IHeatmapService heatmapService,
        IAnalysisNotifier notifier,
        ILogger<VideoAnalysisJob> logger)
    {
        _uow = uow;
        _storage = storage;
        _frameExtractor = frameExtractor;
        _playerDetector = playerDetector;
        _ballTracker = ballTracker;
        _playerTracker = playerTracker;
        _eventDetector = eventDetector;
        _heatmapService = heatmapService;
        _notifier = notifier;
        _logger = logger;
    }


    public async Task AnalyzeAsync(Guid videoId, VideoUploadDto metadata, CancellationToken ct)
    {
        var analysis = await _uow.VideoAnalyses.GetByIdAsync(videoId, ct)
            ?? throw new InvalidOperationException($"Video {videoId} not found");

        try
        {
            await UpdateStatus(analysis, AnalysisStatus.Processing, 5, "Downloading video", ct);

            // === STEP 1: Download video to local disk ===
            var workDir = Path.Combine(Path.GetTempPath(), $"footballai_{videoId}");
            Directory.CreateDirectory(workDir);
            var videoPath = Path.Combine(workDir, "match.mp4");

            using (var stream = await _storage.DownloadAsync(
                $"videos/{videoId}.mp4", ct))
            using (var fs = File.Create(videoPath))
            {
                await stream.CopyToAsync(fs, ct);
            }

            // === STEP 2: Read video metadata ===
            await UpdateStatus(analysis, AnalysisStatus.Processing, 10, "Reading video metadata", ct);
            var videoMeta = await _frameExtractor.GetMetadataAsync(videoPath);
            analysis.VideoDuration = videoMeta.Duration;
            analysis.FramesPerSecond = (int)videoMeta.Fps;

            // === STEP 3: Extract frames ===
            await UpdateStatus(analysis, AnalysisStatus.Processing, 20, "Extracting frames", ct);
            var framesDir = Path.Combine(workDir, "frames");
            var frames = await _frameExtractor.ExtractFramesAsync(
                videoPath, framesDir, targetFps: 5, ct);
            analysis.TotalFramesProcessed = frames.Count;

            // === STEP 4: Detect players in each frame ===
            await UpdateStatus(analysis, AnalysisStatus.Processing, 35, "Detecting players", ct);
            var frameResults = new List<FrameAnalysisResult>();
            var totalFrames = frames.Count;

            for (int i = 0; i < totalFrames; i++)
            {
                ct.ThrowIfCancellationRequested();
                var players = await _playerDetector.DetectPlayersAsync(frames[i], ct);
                var ball = await _ballTracker.TrackBallAsync(frames[i], ct);

                foreach (var p in players) p.FrameIndex = i;
                if (ball is not null) ball.FrameIndex = i;

                frameResults.Add(new FrameAnalysisResult
                {
                    FrameIndex = i,
                    Timestamp = TimeSpan.FromSeconds(i / 5.0),
                    Players = players,
                    Ball = ball
                });

                // Report progress every 5%
                if (i % (totalFrames / 20 + 1) == 0)
                {
                    var pct = 35 + (int)((double)i / totalFrames * 40);
                    await UpdateStatus(analysis, AnalysisStatus.Processing,
                        pct, $"Detecting players ({i}/{totalFrames})", ct);
                }
            }

            // === STEP 5: Create Match record ===
            await UpdateStatus(analysis, AnalysisStatus.Processing, 80, "Building match record", ct);
            var match = new Match
            {
                Title = metadata.Title,
                MatchDate = metadata.MatchDate,
                Venue = metadata.Venue,
                Type = metadata.Type,
                Duration = videoMeta.Duration,
                HomeTeamId = metadata.HomeTeamId,
                AwayTeamId = metadata.AwayTeamId,
                VideoAnalysisId = analysis.Id
            };
            await _uow.Matches.AddAsync(match, ct);

            // === STEP 6: Detect events ===
            await UpdateStatus(analysis, AnalysisStatus.Processing, 85, "Detecting events", ct);
            var events = await _eventDetector.DetectEventsAsync(match.Id, frameResults, ct);
            foreach (var ev in events)
                await _uow.MatchEvents.AddAsync(ev, ct);

            // === STEP 7: Compute per-player statistics ===
            await UpdateStatus(analysis, AnalysisStatus.Processing, 92, "Computing player statistics", ct);
            // Group detections into player tracks (simplified: by team color)
            var tracksByTeam = frameResults
                .SelectMany(f => f.Players)
                .GroupBy(p => p.TeamColor);

            // Real implementation: use DeepSORT for proper player ID tracking
            // and link to Player entities via jersey OCR

            // === STEP 8: Cleanup ===
            await UpdateStatus(analysis, AnalysisStatus.Processing, 97, "Finalizing", ct);
            try { Directory.Delete(workDir, recursive: true); }
            catch (Exception ex) { _logger.LogWarning(ex, "Cleanup failed"); }

            // === DONE ===
            analysis.Status = AnalysisStatus.Completed;
            analysis.ProgressPercent = 100;
            analysis.CurrentStage = "Completed";
            analysis.CompletedAt = DateTime.UtcNow;
            analysis.MatchId = match.Id;
            await _uow.SaveChangesAsync(ct);

            await _notifier.SendCompletedAsync(videoId, match.Id);
            _logger.LogInformation("Analysis completed for video {VideoId}", videoId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Analysis failed for video {VideoId}", videoId);
            analysis.Status = AnalysisStatus.Failed;
            analysis.ErrorMessage = ex.Message;
            await _uow.SaveChangesAsync(ct);
            await _notifier.SendFailedAsync(videoId, ex.Message);
            throw;
        }
    }



    private async Task UpdateStatus(
    VideoAnalysis analysis, AnalysisStatus status, int percent, string stage,
    CancellationToken ct)
    {
        analysis.Status = status;
        analysis.ProgressPercent = percent;
        analysis.CurrentStage = stage;
        if (status == AnalysisStatus.Processing && analysis.StartedAt is null)
            analysis.StartedAt = DateTime.UtcNow;

        await _uow.VideoAnalyses.UpdateAsync(analysis, ct);
        await _uow.SaveChangesAsync(ct);

        await _notifier.SendProgressAsync(analysis.Id, new AnalysisProgressDto
        {
            VideoId = analysis.Id,
            Status = status,
            ProgressPercent = percent,
            CurrentStage = stage
        });
    }

}
