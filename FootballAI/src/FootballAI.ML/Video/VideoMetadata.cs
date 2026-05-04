namespace FootballAI.src.FootballAI.ML.Video;

public class VideoMetadata
{
    public TimeSpan Duration { get; init; }
    public double Fps { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public string CodecName { get; set; } = string.Empty;

}
