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
        _logger.LogInformation("Starting analysis for video {VideoId}", videoId);

        // Load the VideoAnalysis record that was created when the user uploaded.
        // If it doesn't exist, something is very wrong - fail loudly.
        var analysis = await _uow.VideoAnalyses.GetByIdAsync(videoId, ct)
            ?? throw new InvalidOperationException($"Video {videoId} not found");

        // Create a temporary working directory for this video on local disk.
        // Frame extraction needs fast random access - blob storage is too slow.
        var workDir = Path.Combine(Path.GetTempPath(), $"footballai_{videoId}");

        try
        {
            // ============================================================
            // STEP 1 (5%) - DOWNLOAD VIDEO FROM BLOB STORAGE
            // ============================================================
            await UpdateStatus(_uow, _notifier, analysis, AnalysisStatus.Processing, 5,
                "Downloading video from cloud storage", ct);

            Directory.CreateDirectory(workDir);
            var videoPath = Path.Combine(workDir, "match.mp4");

            // Stream the blob to disk - never load multi-GB videos into memory
            using (var blobStream = await _storage.DownloadAsync($"videos/{videoId}.mp4", ct))
            using (var fileStream = File.Create(videoPath))
            {
                await blobStream.CopyToAsync(fileStream, ct);
            }
            _logger.LogInformation("Downloaded video to {Path}", videoPath);

            // ============================================================
            // STEP 2 (10%) - READ VIDEO METADATA
            // ============================================================
            await UpdateStatus(_uow, _notifier, analysis, AnalysisStatus.Processing, 10,
                "Reading video properties", ct);

            var videoMeta = await _frameExtractor.GetMetadataAsync(videoPath);
            analysis.VideoDuration = videoMeta.Duration;
            analysis.FramesPerSecond = (int)videoMeta.Fps;

            _logger.LogInformation(
                "Video: {Duration} duration, {Fps} FPS, {Width}x{Height}",
                videoMeta.Duration, videoMeta.Fps, videoMeta.Width, videoMeta.Height);

            // ============================================================
            // STEP 3 (20%) - EXTRACT FRAMES
            // ============================================================
            // We extract at 5 FPS because:
            //   - 30 FPS source produces too many redundant frames
            //   - 5 FPS gives enough temporal resolution for tracking
            //   - Reduces compute by 6x compared to processing every frame
            await UpdateStatus(_uow, _notifier, analysis, AnalysisStatus.Processing, 20,
                "Extracting frames from video", ct);

            var framesDir = Path.Combine(workDir, "frames");
            var frames = await _frameExtractor.ExtractFramesAsync(
                videoPath, framesDir, targetFps: 5, ct);
            analysis.TotalFramesProcessed = frames.Count;

            _logger.LogInformation("Extracted {Count} frames", frames.Count);

            // ============================================================
            // STEP 4 (35-75%) - DETECT PLAYERS & TRACK BALL
            // ============================================================
            // This is the most CPU-intensive part - YOLO inference on every frame.
            // We report progress every ~5% so the user sees movement in the UI.
            await UpdateStatus(_uow, _notifier, analysis, AnalysisStatus.Processing, 35,
                "Detecting players and tracking ball", ct);

            var frameResults = new List<FrameAnalysisResult>();
            var totalFrames = frames.Count;

            for (int i = 0; i < totalFrames; i++)
            {
                // Allow user/admin to cancel a long-running job
                ct.ThrowIfCancellationRequested();

                // Run YOLO + ball detection on this single frame
                var players = await _playerDetector.DetectPlayersAsync(frames[i], ct);
                var ball = await _ballTracker.TrackBallAsync(frames[i], ct);

                // Tag detections with frame index for later sequencing
                foreach (var p in players) p.FrameIndex = i;
                if (ball is not null) ball.FrameIndex = i;

                frameResults.Add(new FrameAnalysisResult
                {
                    FrameIndex = i,
                    Timestamp = TimeSpan.FromSeconds(i / 5.0),
                    Players = players,
                    Ball = ball
                });

                // Update progress every 5% so the user sees responsive UI
                if (i % Math.Max(1, totalFrames / 20) == 0)
                {
                    var pct = 35 + (int)((double)i / totalFrames * 40);
                    await UpdateStatus(_uow, _notifier, analysis, AnalysisStatus.Processing, pct,
                        $"Analyzing frame {i + 1} of {totalFrames}", ct);
                }
            }

            // ============================================================
            // STEP 5 (80%) - CREATE THE MATCH RECORD
            // ============================================================
            await UpdateStatus(_uow, _notifier, analysis, AnalysisStatus.Processing, 80,
                "Building match record", ct);

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

            // ============================================================
            // STEP 6 (85%) - DETECT MATCH EVENTS
            // ============================================================
            // Now that we have all frame data, look for patterns that
            // represent goals, shots, passes, etc.
            await UpdateStatus(_uow, _notifier, analysis, AnalysisStatus.Processing, 85,
                "Detecting match events", ct);

            var events = await _eventDetector.DetectEventsAsync(match.Id, frameResults, ct);
            foreach (var ev in events)
                await _uow.MatchEvents.AddAsync(ev, ct);

            _logger.LogInformation("Detected {Count} match events", events.Count);

            // ============================================================
            // STEP 7 (92%) - COMPUTE PER-PLAYER STATISTICS
            // ============================================================
            // Group all detections by player track (jersey color + tracking ID).
            // Real production: use DeepSORT with jersey OCR for player ID.
            await UpdateStatus(_uow, _notifier, analysis, AnalysisStatus.Processing, 92,
                "Computing player statistics", ct);

            var allPlayerDetections = frameResults.SelectMany(f => f.Players).ToList();
            var playerTracks = GroupIntoPlayerTracks(allPlayerDetections);

            foreach (var track in playerTracks)
            {
                var stats = _playerTracker.ComputePlayerStats(
                    track.Detections,
                    fps: 5.0,
                    metersPerPixel: ComputeFieldScale(videoMeta.Width));

                stats.MatchId = match.Id;
                // In production: link stats.PlayerId via jersey number recognition
                match.PlayerStats.Add(stats);
            }

            // ============================================================
            // STEP 8 (97%) - CLEANUP TEMP FILES
            // ============================================================
            await UpdateStatus(_uow, _notifier, analysis, AnalysisStatus.Processing, 97,
                "Finalizing analysis", ct);

            try { Directory.Delete(workDir, recursive: true); }
            catch (Exception ex) { _logger.LogWarning(ex, "Failed to clean up {Dir}", workDir); }

            // ============================================================
            // FINAL - MARK COMPLETE & NOTIFY USER
            // ============================================================
            analysis.Status = AnalysisStatus.Completed;
            analysis.ProgressPercent = 100;
            analysis.CurrentStage = "Completed";
            analysis.CompletedAt = DateTime.UtcNow;
            analysis.MatchId = match.Id;

            await _uow.SaveChangesAsync(ct);

            // SignalR push - the frontend dashboard updates instantly
            await _notifier.SendCompletedAsync(videoId, match.Id);
            _logger.LogInformation("Analysis completed for video {VideoId}", videoId);
        }
        catch (OperationCanceledException)
        {
            // The job was cancelled (admin stop, app shutdown, etc.)
            _logger.LogWarning("Analysis cancelled for video {VideoId}", videoId);
            analysis.Status = AnalysisStatus.Cancelled;
            analysis.ErrorMessage = "Analysis was cancelled";
            await _uow.SaveChangesAsync(CancellationToken.None);
            throw;  // re-throw so Hangfire marks it cancelled, not failed
        }
        catch (Exception ex)
        {
            // ============================================================
            // ERROR HANDLING
            // ============================================================
            // If anything fails, mark the analysis as Failed and notify the user.
            // Hangfire will automatically retry up to 10 times by default.
            _logger.LogError(ex, "Analysis failed for video {VideoId}", videoId);

            analysis.Status = AnalysisStatus.Failed;
            analysis.ErrorMessage = ex.Message;
            await _uow.SaveChangesAsync(CancellationToken.None);

            await _notifier.SendFailedAsync(videoId, ex.Message);

            // Cleanup temp files even on failure
            try { if (Directory.Exists(workDir)) Directory.Delete(workDir, recursive: true); }
            catch { /* best effort */ }

            throw;  // re-throw so Hangfire's retry logic kicks in
        }
    }


}
