namespace FootballAI.Application.DTOs;

public class DetectedPlayerDto
{
    public Guid? PlayerId { get; set; }
    public int FrameIndex { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public string TeamColor { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public int? JerseyNumber { get; set; }
}
