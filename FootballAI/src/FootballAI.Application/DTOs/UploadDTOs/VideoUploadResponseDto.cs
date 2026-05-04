namespace FootballAI.Application.DTOs;

public class VideoUploadResponseDto
{
    public Guid VideoId { get; set; }
    public string JobId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
