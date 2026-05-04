using FootballAI.Application.DTOs;
using FootballAI.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace FootballAI.src.FootballAI.ML.Detectors;

public class PlayerDetector : IPlayerDetectionService,IDisposable
{
    private readonly InferenceSession _session;
    private readonly ILogger<PlayerDetector> _logger;
    private const int InputWidth = 640;
    private const int InputHeight = 640;
    private const float ConfidenceThreshold = 0.5f;
    private const float IouThreshold = 0.45f;
    private const int PersonClassId = 0;

    public PlayerDetector(string modelPath, ILogger<PlayerDetector> logger)
    {
        _logger = logger;
        var options = new SessionOptions
        {
            GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL,
            ExecutionMode = ExecutionMode.ORT_PARALLEL
        };
        // Optionally enable CUDA for GPU acceleration:
        // options.AppendExecutionProvider_CUDA();
        _session = new InferenceSession(modelPath, options);
        _logger.LogInformation("Loaded YOLO model from {Path}", modelPath);
    }

    public async Task<List<DetectedPlayerDto>> DetectPlayersAsync(
         string framePath, CancellationToken ct = default)
    {
        using var image = await Image.LoadAsync<Rgb24>(framePath, ct);
        var origWidth = image.Width;
        var origHeight = image.Height;

        // Resize to model input size
        image.Mutate(x => x.Resize(InputWidth, InputHeight));

        // Convert image to tensor
        var inputTensor = ImageToTensor(image);
        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("images", inputTensor)
        };

        // Run inference
        using var results = _session.Run(inputs);
        var output = results.First().AsTensor<float>();

        // Parse YOLO output
        var detections = ParseYoloOutput(output, origWidth, origHeight);

        // Apply Non-Maximum Suppression
        var filtered = ApplyNms(detections);

        // Determine team color for each detection
        using var colorImage = await Image.LoadAsync<Rgb24>(framePath, ct);
        foreach (var det in filtered)
        {
            det.TeamColor = ClassifyJerseyColor(colorImage, det);
        }

        return filtered;
    }

    private DenseTensor<float> ImageToTensor(Image<Rgb24> image)
    {
        var tensor = new DenseTensor<float>(new[] { 1, 3, InputHeight, InputWidth });
        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (int x = 0; x < row.Length; x++)
                {
                    tensor[0, 0, y, x] = row[x].R / 255.0f;
                    tensor[0, 1, y, x] = row[x].G / 255.0f;
                    tensor[0, 2, y, x] = row[x].B / 255.0f;
                }
            }
        });
        return tensor;
    }

    private List<DetectedPlayerDto> ParseYoloOutput(
      Tensor<float> output, int origWidth, int origHeight)
    {
        var detections = new List<DetectedPlayerDto>();
        var dimensions = output.Dimensions.ToArray();

        // YOLOv8 output shape: [1, 84, 8400] -> 80 classes + 4 box coords
        var numBoxes = dimensions[2];
        var scaleX = (float)origWidth / InputWidth;
        var scaleY = (float)origHeight / InputHeight;

        for (int i = 0; i < numBoxes; i++)
        {
            // Get max class confidence (skip box coords at indices 0-3)
            float maxConf = 0;
            int classId = -1;
            for (int c = 0; c < 80; c++)
            {
                var conf = output[0, 4 + c, i];
                if (conf > maxConf)
                {
                    maxConf = conf;
                    classId = c;
                }
            }

            if (classId != PersonClassId || maxConf < ConfidenceThreshold) continue;

            var cx = output[0, 0, i] * scaleX;
            var cy = output[0, 1, i] * scaleY;
            var w = output[0, 2, i] * scaleX;
            var h = output[0, 3, i] * scaleY;

            detections.Add(new DetectedPlayerDto
            {
                X = cx - w / 2,
                Y = cy - h / 2,
                Width = w,
                Height = h,
                Confidence = maxConf
            });
        }
        return detections;
    }

    private List<DetectedPlayerDto> ApplyNms(List<DetectedPlayerDto> detections)
    {
        var sorted = detections.OrderByDescending(d => d.Confidence).ToList();
        var keep = new List<DetectedPlayerDto>();

        while (sorted.Count > 0)
        {
            var best = sorted[0];
            keep.Add(best);
            sorted.RemoveAt(0);
            sorted = sorted.Where(d => CalculateIou(best, d) < IouThreshold).ToList();
        }
        return keep;
    }
    private float CalculateIou(DetectedPlayerDto a, DetectedPlayerDto b)
    {
        var xA = Math.Max(a.X, b.X);
        var yA = Math.Max(a.Y, b.Y);
        var xB = Math.Min(a.X + a.Width, b.X + b.Width);
        var yB = Math.Min(a.Y + a.Height, b.Y + b.Height);

        var interArea = Math.Max(0, xB - xA) * Math.Max(0, yB - yA);
        var unionArea = (a.Width * a.Height) + (b.Width * b.Height) - interArea;
        return unionArea > 0 ? (float)(interArea / unionArea) : 0;
    }


    private string ClassifyJerseyColor(Image<Rgb24> image, DetectedPlayerDto detection)
    {
        // Sample upper-body region (jersey area) - top 40% of bounding box
        var x = (int)Math.Max(0, detection.X);
        var y = (int)Math.Max(0, detection.Y);
        var w = (int)Math.Min(image.Width - x, detection.Width);
        var h = (int)(detection.Height * 0.4);
        h = Math.Min(image.Height - y, h);

        if (w <= 0 || h <= 0) return "unknown";

        long rSum = 0, gSum = 0, bSum = 0, count = 0;
        image.ProcessPixelRows(accessor =>
        {
            for (int py = y; py < y + h && py < accessor.Height; py++)
            {
                var row = accessor.GetRowSpan(py);
                for (int px = x; px < x + w && px < row.Length; px++)
                {
                    rSum += row[px].R;
                    gSum += row[px].G;
                    bSum += row[px].B;
                    count++;
                }
            }
        });

        if (count == 0) return "unknown";

        var avgR = rSum / count;
        var avgG = gSum / count;
        var avgB = bSum / count;

        return ColorToTeam(avgR, avgG, avgB);
    }

    private string ColorToTeam(long r, long g, long b)
    {
        // Simple heuristic - in production, configure team colors per match
        if (r > 150 && g < 100 && b < 100) return "red";
        if (b > 150 && r < 100 && g < 130) return "blue";
        if (r > 200 && g > 200 && b > 200) return "white";
        if (r < 60 && g < 60 && b < 60) return "black";
        if (g > 150 && r < 150 && b < 100) return "green";
        if (r > 200 && g > 200 && b < 100) return "yellow";
        return "unknown";
    }

    public void Dispose() => _session?.Dispose();
}

