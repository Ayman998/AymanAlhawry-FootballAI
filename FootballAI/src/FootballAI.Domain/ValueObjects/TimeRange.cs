namespace FootballAI.Domain.ValueObjects;

public record TimeRange(TimeSpan Start, TimeSpan End)
{
    public TimeSpan Duration => End - Start;
    public bool Contains(TimeSpan time) => time >= Start && time <= End;
}
