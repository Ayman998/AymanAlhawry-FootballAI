using FootballAI.src.FootballAI.Domain.ValueObjects;

namespace FootballAI.Domain.ValueObjects;

public record BoundingBox(double X, double Y, double Width, double Height)
{
    public Position Center => new(X + Width / 2, Y + Height / 2);
    public double Area => Width * Height;
}
