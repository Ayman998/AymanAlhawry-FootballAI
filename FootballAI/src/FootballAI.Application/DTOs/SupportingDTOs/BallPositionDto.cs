namespace FootballAI.Application.DTOs;

public class BallPositionDto
{
    public int FrameIndex { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Confidence { get; set; }
}
