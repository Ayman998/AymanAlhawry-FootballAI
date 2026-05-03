using FootballAI.Application.DTOs;
using FootballAI.Application.Interfaces;
using FootballAI.src.FootbalaAI.API.Hub;
using Microsoft.AspNetCore.SignalR;

namespace FootballAI.src.FootbalaAI.API.Services;

public class AnalysisNotifier : IAnalysisNotifier
{
    private readonly IHubContext<AnalysisHub> _hubContext;

    public AnalysisNotifier(IHubContext<AnalysisHub> hubContext) => _hubContext = hubContext;

    public Task SendProgressAsync(Guid videoId, AnalysisProgressDto progress)
        => _hubContext.Clients.Group(videoId.ToString())
            .SendAsync("AnalysisProgress", progress);

    public Task SendCompletedAsync(Guid videoId, Guid matchId)
        => _hubContext.Clients.Group(videoId.ToString())
            .SendAsync("AnalysisCompleted", new { videoId, matchId });

    public Task SendFailedAsync(Guid videoId, string error)
        => _hubContext.Clients.Group(videoId.ToString())
            .SendAsync("AnalysisFailed", new { videoId, error });
}
