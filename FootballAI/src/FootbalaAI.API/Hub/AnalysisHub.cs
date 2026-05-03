namespace FootballAI.src.FootbalaAI.API.Hub;

public class AnalysisHub : Microsoft.AspNetCore.SignalR.Hub
{
    public async Task JoinVideoGroup(string videoId)
    => await Groups.AddToGroupAsync(Context.ConnectionId, videoId);

    public async Task LeaveVideoGroup(string videoId)
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, videoId);
}
