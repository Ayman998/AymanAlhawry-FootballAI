namespace FootballAI.src.FootballAI.Application.DTOs;

public class VideoUploadDto
{
    public string Title { get; set; } = string.Empty;
    public DateTime MatchDate { get; set; }
    public string Venue { get; set; } = string.Empty;
    public Guid HomeTeamId { get; set; }
    public Guid AwayTeamId { get; set; }
    public MatchType Type { get; set; }
}


public class VideoUploadResponseDto
{
    public Guid VideoId { get; set; }
    public string JobId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}