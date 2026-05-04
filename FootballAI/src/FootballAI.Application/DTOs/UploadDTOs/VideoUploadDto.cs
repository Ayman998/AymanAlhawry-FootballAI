namespace FootballAI.Application.DTOs;

public class VideoUploadDto
{
    public string Title { get; set; } = string.Empty;
    public DateTime MatchDate { get; set; }
    public string Venue { get; set; } = string.Empty;
    public Guid HomeTeamId { get; set; }
    public Guid AwayTeamId { get; set; }
    public Domain.Enums.MatchType Type { get; set; }
}

