using FootballAI.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text;

namespace FootballAI.Application.Services;

public class ReportService : IReportService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<ReportService> _logger;

    public ReportService(IUnitOfWork uow, ILogger<ReportService> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<byte[]> GeneratePdfReportAsync(Guid matchId, CancellationToken ct = default)
    {
        var match = await _uow.Matches.GetByIdAsync(matchId, ct)
       ?? throw new KeyNotFoundException($"Match {matchId} not found");

        // Use a library like QuestPDF here
        // Returning placeholder - real implementation would render actual PDF
        _logger.LogInformation("Generating PDF report for match {MatchId}", matchId);
        return Array.Empty<byte>();
    }

    public async Task<byte[]> GenerateExcelReportAsync(Guid matchId, CancellationToken ct = default)
    {
        var match = await _uow.Matches.GetByIdAsync(matchId, ct)
          ?? throw new KeyNotFoundException($"Match {matchId} not found");

        // Use ClosedXML or EPPlus here
        _logger.LogInformation("Generating Excel report for match {MatchId}", matchId);
        return Array.Empty<byte>();
    }
}
