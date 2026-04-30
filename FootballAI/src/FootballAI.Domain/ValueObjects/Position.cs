namespace FootballAI.Domain.ValueObjects;

public record Position(double X, double Y)
{
    public double DistanceTo(Position other)
    {
        var dx = X - other.X;
        var dy = Y - other.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    public static Position Origin => new(0, 0);
}
