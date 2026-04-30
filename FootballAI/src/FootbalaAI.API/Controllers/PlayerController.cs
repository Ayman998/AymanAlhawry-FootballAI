using FootballAI.Application.Interfaces;
using FootballAI.src.FootballAI.Application.DTOs;
using FootballAI.src.FootballAI.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballAI.src.FootbalaAI.API.Controllers;

[ApiController]
[Route("api/[controller]")  ]
[Authorize]
public class PlayerController :ControllerBase
{
    private readonly IUnitOfWork _uow;

    public PlayerController(IUnitOfWork uow) => _uow = uow;

    [HttpGet("{playerId}")]
    public async Task<ActionResult<Player>> GetPlayer(Guid playerId, CancellationToken ct)
    {
        var player = await _uow.Players.GetByIdAsync(playerId, ct);
        return player is null ? NotFound() : Ok(player);
    }

    [HttpGet("{playerId}/stats/{matchId}")]
    public async Task<ActionResult<PlayerStatsDto>> GetMatchStats(
        Guid playerId, Guid matchId, CancellationToken ct)
    {
        var player = await _uow.Players.GetByIdAsync(playerId, ct);
        if (player is null) return NotFound("Player not found");

        var stats = player.Stats.FirstOrDefault(s => s.MatchId == matchId);
        if (stats is null) return NotFound("Stats not available for this match");

        return Ok(new PlayerStatsDto
        {
            PlayerId = player.Id,
            PlayerName = player.FullName,
            JerseyNumber = player.JerseyNumber,
            Position = player.Position.ToString(),
            TotalDistanceKm = stats.TotalDistanceKm,
            MaxSpeedKmh = stats.MaxSpeedKmh,
            AverageSpeedKmh = stats.AverageSpeedKmh,
            SprintCount = stats.SprintCount,
            Touches = stats.Touches,
            PassesAttempted = stats.PassesAttempted,
            PassesCompleted = stats.PassesCompleted,
            PassAccuracy = stats.PassAccuracy,
            Goals = stats.Goals,
            Assists = stats.Assists,
            Shots = stats.Shots,
            Tackles = stats.Tackles,
            TimeOnPitch = stats.TimeOnPitch
        });
    }
}
