namespace FootballAI.Application.Interfaces;

public interface IReportService
{
    Task<byte[]> GeneratePdfReportAsync(Guid matchId, CancellationToken ct = default);
    Task<byte[]> GenerateExcelReportAsync(Guid matchId, CancellationToken ct = default);
}
