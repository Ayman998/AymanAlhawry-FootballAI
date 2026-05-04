using FootballAI.Application.DTOs;
using FootballAI.Application.Interfaces;
using FootballAI.Domain.Entities;
using FootballAI.Domain.Enums;

namespace FootballAI.Application.Helpers;

public static class AnalysisStatusHelper
{
    public static async Task UpdateStatus(IUnitOfWork uow, IAnalysisNotifier notifier, VideoAnalysis analysis,
            AnalysisStatus status,
            int percent,
            string stage,
            CancellationToken ct)
    {
        analysis.Status = status;
        analysis.ProgressPercent = percent;
        analysis.CurrentStage = stage;

        if (status == AnalysisStatus.Processing && analysis.StartedAt is null)
            analysis.StartedAt = DateTime.UtcNow;

        await uow.VideoAnalyses.UpdateAsync(analysis, ct);
        await uow.SaveChangesAsync(ct);

        await notifier.SendProgressAsync(analysis.Id, new AnalysisProgressDto
        {
            VideoId = analysis.Id,
            Status = status,
            ProgressPercent = percent,
            CurrentStage = stage
        });
    }
}
