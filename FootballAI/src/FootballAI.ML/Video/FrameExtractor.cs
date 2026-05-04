using FFMpegCore;
using Microsoft.Extensions.Logging;

namespace FootballAI.src.FootballAI.ML.Video;

public class FrameExtractor
{
    private readonly ILogger<FrameExtractor> _logger;

    public FrameExtractor(ILogger<FrameExtractor> logger)
    {
        _logger = logger;
    }

    public async Task<List<string>> ExtractFramesAsync(
    string videoPath,
    string outputDirectory,
    int targetFps = 5,
    CancellationToken ct = default)
    {
        Directory.CreateDirectory(outputDirectory);
        var outputPattern = Path.Combine(outputDirectory, "frame_%05d.jpg");

        _logger.LogInformation("Extracting frames from {Video} at {Fps} FPS", videoPath, targetFps);

        await FFMpegArguments
            .FromFileInput(videoPath)
            .OutputToFile(outputPattern, overwrite: true, opts => opts
                .WithVideoFilters(f => f.Scale(1920, 1080))
                .WithFramerate(targetFps)
                .ForceFormat("image2"))
            .CancellableThrough(ct)
            .ProcessAsynchronously();

        var frames = Directory.GetFiles(outputDirectory, "frame_*.jpg")
                              .OrderBy(f => f)
                              .ToList();

        _logger.LogInformation("Extracted {Count} frames", frames.Count);
        return frames;
    }

    public async Task<VideoMetadata> GetMetadataAsync(string videoPath)
    {
        var info = await FFProbe.AnalyseAsync(videoPath);
        return new VideoMetadata
        {
            Duration = info.Duration,
            Width = info.PrimaryVideoStream?.Width ?? 0,
            Height = info.PrimaryVideoStream?.Height ?? 0,
            Fps = info.PrimaryVideoStream?.FrameRate ?? 30,
            CodecName = info.PrimaryVideoStream?.CodecName ?? "unknown"
        };
    }
}
