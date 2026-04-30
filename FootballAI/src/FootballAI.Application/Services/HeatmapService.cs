using FootballAI.Application.DTOs;
using FootballAI.Application.Interfaces;

namespace FootballAI.Application.Services;

public class HeatmapService : IHeatmapService
{
    private const int GridRows = 10;
    private const int GridCols = 10;

    public List<HeatmapPointDto> GenerateHeatmap(IEnumerable<(double X, double Y)> positions)
    {
        // Bucket positions into a grid and compute intensity per cell
        var grid = new double[GridRows, GridCols];
        var positionList = positions.ToList();

        foreach (var (x, y) in positionList)
        {
            int col = Math.Clamp((int)(x * GridCols), 0, GridCols - 1);
            int row = Math.Clamp((int)(y * GridRows), 0, GridRows - 1);
            grid[row, col]++;
        }

        double max = positionList.Count > 0
            ? positionList.Max(_ => grid[
                Math.Clamp((int)(_.Y * GridRows), 0, GridRows - 1),
                Math.Clamp((int)(_.X * GridCols), 0, GridCols - 1)])
            : 1;

        var result = new List<HeatmapPointDto>();
        for (int r = 0; r < GridRows; r++)
        {
            for (int c = 0; c < GridCols; c++)
            {
                if (grid[r, c] > 0)
                {
                    result.Add(new HeatmapPointDto
                    {
                        X = (c + 0.5) / GridCols,
                        Y = (r + 0.5) / GridRows,
                        Intensity = max > 0 ? grid[r, c] / max : 0
                    });
                }
            }
        }

        return result;
    }
}
