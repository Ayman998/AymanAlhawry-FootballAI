using FootballAI.Application.DTOs;
using FootballAI.Application.Interfaces;
using FootballAI.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballAI.src.FootbalaAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MatchController :ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly IReportService _reports;

    public MatchController(IUnitOfWork uow, IReportService reports)
    {
        _uow = uow;
        _reports = reports;
    }

    [HttpGet("{matchId}")]
    public async Task<ActionResult<MatchAnalysisDto>> GetMatch(
      Guid matchId, CancellationToken ct)
    {
        var match = await _uow.Matches.GetByIdAsync(matchId, ct);
        if (match is null) return NotFound();

        var dto = new MatchAnalysisDto
        {
            MatchId = match.Id,
            Title = match.Title,
            MatchDate = match.MatchDate,
            HomeTeam = match.HomeTeam?.Name ?? "Unknown",
            AwayTeam = match.AwayTeam?.Name ?? "Unknown",
            HomeScore = match.HomeScore,
            AwayScore = match.AwayScore,
            Status = match.VideoAnalysis?.Status ?? AnalysisStatus.Pending,
            ProgressPercent = match.VideoAnalysis?.ProgressPercent ?? 0
        };
        return Ok(dto);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MatchAnalysisDto>>> GetAll(CancellationToken ct)
    {
        var matches = await _uow.Matches.GetAllAsync(ct);
        var dtos = matches.Select(m => new MatchAnalysisDto
        {
            MatchId = m.Id,
            Title = m.Title,
            MatchDate = m.MatchDate,
            HomeScore = m.HomeScore,
            AwayScore = m.AwayScore
        });
        return Ok(dtos);
    }

    [HttpGet("{matchId}/report/pdf")]
    public async Task<IActionResult> DownloadPdfReport(Guid matchId, CancellationToken ct)
    {
        var bytes = await _reports.GeneratePdfReportAsync(matchId, ct);
        return File(bytes, "application/pdf", $"match-{matchId}-report.pdf");
    }

    [HttpGet("{matchId}/report/excel")]
    public async Task<IActionResult> DownloadExcelReport(Guid matchId, CancellationToken ct)
    {
        var bytes = await _reports.GenerateExcelReportAsync(matchId, ct);
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"match-{matchId}-report.xlsx");
    }
}
