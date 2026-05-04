using AutoMapper;
using FootballAI.Application.DTOs;
using FootballAI.Application.DTOs.MatchDTOs;
using FootballAI.Domain.Entities;

namespace FootballAI.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Player, PlayerStatsDto>()
            .ForMember(d => d.PlayerName, o => o.MapFrom(s => s.FullName))
            .ForMember(d => d.Position, o => o.MapFrom(s => s.Position.ToString()))
            .ForMember(d => d.TotalDistanceKm, o => o.Ignore())
            .ForMember(d => d.MaxSpeedKmh, o => o.Ignore())
            .ForMember(d => d.AverageSpeedKmh, o => o.Ignore())
            .ForMember(d => d.SprintCount, o => o.Ignore())
            .ForMember(d => d.Touches, o => o.Ignore())
            .ForMember(d => d.PassesAttempted, o => o.Ignore())
            .ForMember(d => d.PassesCompleted, o => o.Ignore())
            .ForMember(d => d.PassAccuracy, o => o.Ignore())
            .ForMember(d => d.Goals, o => o.Ignore())
            .ForMember(d => d.Assists, o => o.Ignore())
            .ForMember(d => d.Shots, o => o.Ignore())
            .ForMember(d => d.Tackles, o => o.Ignore())
            .ForMember(d => d.TimeOnPitch, o => o.Ignore())
            .ForMember(d => d.HeatmapPoints, o => o.Ignore())
            .ForMember(d => d.TeamName, o => o.MapFrom(s => s.Team != null ? s.Team.Name : string.Empty));

        CreateMap<Team, TeamStatsDto>()
            .ForMember(d => d.TeamId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.TeamName, o => o.MapFrom(s => s.Name))
            .ForMember(d => d.PossessionPercent, o => o.Ignore())
            .ForMember(d => d.TotalPasses, o => o.Ignore())
            .ForMember(d => d.CompletedPasses, o => o.Ignore())
            .ForMember(d => d.PassAccuracy, o => o.Ignore())
            .ForMember(d => d.Shots, o => o.Ignore())
            .ForMember(d => d.ShotsOnTarget, o => o.Ignore())
            .ForMember(d => d.Corners, o => o.Ignore())
            .ForMember(d => d.Fouls, o => o.Ignore())
            .ForMember(d => d.Formation, o => o.Ignore())
            .ForMember(d => d.TotalDistanceKm, o => o.Ignore());

        CreateMap<Match, MatchAnalysisDto>()
            .ForMember(d => d.MatchId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.HomeTeam, o => o.MapFrom(s => s.HomeTeam != null ? s.HomeTeam.Name : string.Empty))
            .ForMember(d => d.AwayTeam, o => o.MapFrom(s => s.AwayTeam != null ? s.AwayTeam.Name : string.Empty))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.VideoAnalysis != null
                ? s.VideoAnalysis.Status
                : Domain.Enums.AnalysisStatus.Pending))
            .ForMember(d => d.ProgressPercent, o => o.MapFrom(s => s.VideoAnalysis != null
                ? s.VideoAnalysis.ProgressPercent
                : 0))
            .ForMember(d => d.Events, o => o.Ignore())
            .ForMember(d => d.HomeTeamStats, o => o.Ignore())
            .ForMember(d => d.AwayTeamStats, o => o.Ignore());

        CreateMap<MatchEvent, MatchEventDto>()
            .ForMember(d => d.PlayerName, o => o.MapFrom(s => s.Player != null ? s.Player.FullName : string.Empty));
    }
}
