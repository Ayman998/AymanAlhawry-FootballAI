using FootballAI.Application.DTOs;
using FootballAI.src.FootballAI.Application.DTOs;
using FootballAI.src.FootballAI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballAI.src.FootbalaAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VideoController : ControllerBase
{
    private readonly IVideoProcessingService _videoService;
    private readonly ILogger<VideoController> _logger;

    public VideoController(IVideoProcessingService videoService,
        ILogger<VideoController> logger)
    {
        _videoService = videoService;
        _logger = logger;
    }


    [HttpPost("upload")]
    [RequestSizeLimit(5_368_709_120)] // Size is 5 GB
    [RequestFormLimits(MultipartBodyLengthLimit = 5_368_709_120)]

    public async Task<ActionResult<VideoUploadResponseDto>> Upload(
        IFormFile videoFile,
        [FromForm] VideoUploadDto metadata,
        CancellationToken ct)
    {
        try
        {
            if (videoFile is null || videoFile.Length == 0)
                return BadRequest("No video file provided");

            var result = await _videoService.ProcessVideoAsync(videoFile, metadata, ct);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Video upload failed");
            return StatusCode(500, "Video upload failed");
        }
    }


    [HttpGet("{videoId}/progress")]
    public async Task<ActionResult<AnalysisProgressDto>> GetProgress(
        Guid videoId, CancellationToken ct)
    {
        try
        {
            var progress = await _videoService.GetProgressAsync(videoId, ct);
            return Ok(progress);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

}


