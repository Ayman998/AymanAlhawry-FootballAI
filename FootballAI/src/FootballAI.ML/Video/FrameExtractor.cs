using FFMpegCore;

namespace FootballAI.src.FootballAI.ML.Video;

public class FrameExtractor
{
    public async Task<VideoMetadata> GetMetadataAsync(string videoPath)
    {
        var info = await FFProbe.AnalyseAsync(videoPath);
        var video = info.PrimaryVideoStream
            ?? throw new InvalidOperationException($"No video stream found in '{videoPath}'.");

        return new VideoMetadata
        {
            Duration = info.Duration,
            Fps      = video.AvgFrameRate,
            Width    = video.Width,
            Height   = video.Height
        };
    }

    public async Task<List<string>> ExtractFramesAsync(
        string videoPath,
        string outputDir,
        int targetFps,
        CancellationToken ct)
    {
        Directory.CreateDirectory(outputDir);

        var outputPattern = Path.Combine(outputDir, "frame_%06d.jpg");

        await FFMpegArguments
            .FromFileInput(videoPath)
            .OutputToFile(outputPattern, overwrite: true, options => options
                .WithFramerate(targetFps)
                .ForceFormat("image2"))
            .CancellableThrough(ct)
            .ProcessAsynchronously();

        return Directory
            .GetFiles(outputDir, "frame_*.jpg")
            .OrderBy(f => f)
            .ToList();
    }
}
