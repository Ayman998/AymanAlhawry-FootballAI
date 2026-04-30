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

        _logger.LogInformation("Generating PDF report for match {MatchId}", matchId);

        // Placeholder: return a minimal PDF-like byte array
        // Replace with a real PDF library (e.g., QuestPDF, iText) in production
        var content = $"Match Report\nMatch ID: {matchId}\nTitle: {match.Title}";
        return Encoding.UTF8.GetBytes(content);
    }

    public async Task<byte[]> GenerateExcelReportAsync(Guid matchId, CancellationToken ct = default)
    {
        var match = await _uow.Matches.GetByIdAsync(matchId, ct)
            ?? throw new KeyNotFoundException($"Match {matchId} not found");

        _logger.LogInformation("Generating Excel report for match {MatchId}", matchId);

        // Placeholder: return CSV-formatted bytes
        // Replace with a real Excel library (e.g., ClosedXML, EPPlus) in production
        var csv = new StringBuilder();
        csv.AppendLine("MatchId,Title,MatchDate");
        csv.AppendLine($"{match.Id},{match.Title},{match.MatchDate:yyyy-MM-dd}");
        return Encoding.UTF8.GetBytes(csv.ToString());
    }
}
