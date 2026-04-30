using FootballAI.Application.DTOs;
using FootballAI.Application.Interfaces;
using FootballAI.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballAI.src.FootbalaAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TeamController :ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly ITeamAnalysisService _analyzer;

    public TeamController(IUnitOfWork uow, ITeamAnalysisService analyzer)
    {
        _uow = uow;
        _analyzer = analyzer;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Team>>> GetAll(CancellationToken ct)
        => Ok(await _uow.Teams.GetAllAsync(ct));

    [HttpGet("{teamId}")]
    public async Task<ActionResult<Team>> GetTeam(Guid teamId, CancellationToken ct)
    {
        var team = await _uow.Teams.GetByIdAsync(teamId, ct);
        return team is null ? NotFound() : Ok(team);
    }

    [HttpGet("{teamId}/match/{matchId}/stats")]
    public async Task<ActionResult<TeamStatsDto>> GetTeamMatchStats(
        Guid teamId, Guid matchId, CancellationToken ct)
    {
        var stats = await _analyzer.AnalyzeTeamAsync(matchId, teamId, ct);
        return Ok(stats);
    }
}
