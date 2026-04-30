using FootballAI.Application.DTOs;

namespace FootballAI.Application.Interfaces;

public interface IHeatmapService
{
    List<HeatmapPointDto> GenerateHeatmap(IEnumerable<(double X, double Y)> positions);
}
