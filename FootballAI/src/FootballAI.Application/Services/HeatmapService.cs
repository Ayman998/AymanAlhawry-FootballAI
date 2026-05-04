using FootballAI.Application.DTOs;
using FootballAI.Application.Interfaces;

namespace FootballAI.Application.Services;

public class HeatmapService : IHeatmapService
{
    private const int GridWidth = 50;
    private const int GridHeight = 32;

    public List<HeatmapPointDto> GenerateHeatmap(IEnumerable<(double X, double Y)> positions)
    {
        var grid = new int[GridWidth, GridHeight];
        var positionList = positions.ToList();

        if (positionList.Count == 0) return new List<HeatmapPointDto>();

        // Normalize positions to grid (assumes 1920x1080 frame)
        const double frameWidth = 1920;
        const double frameHeight = 1080;

        foreach (var (x, y) in positionList)
        {
            var gx = (int)Math.Min(GridWidth - 1, Math.Max(0, x / frameWidth * GridWidth));
            var gy = (int)Math.Min(GridHeight - 1, Math.Max(0, y / frameHeight * GridHeight));
            grid[gx, gy]++;
        }

        var maxCount = grid.Cast<int>().Max();
        var points = new List<HeatmapPointDto>();

        for (int x = 0; x < GridWidth; x++)
            for (int y = 0; y < GridHeight; y++)
            {
                if (grid[x, y] == 0) continue;
                points.Add(new HeatmapPointDto
                {
                    X = x * (frameWidth / GridWidth),
                    Y = y * (frameHeight / GridHeight),
                    Intensity = (double)grid[x, y] / maxCount
                });
            }
        return points;
    }
}
