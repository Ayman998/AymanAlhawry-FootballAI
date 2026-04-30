using FootballAI.Domain.Common;
using FootballAI.Domain.Enums;

namespace FootballAI.Domain.Entities;

public class Player : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public int JerseyNumber { get; set; }
    public PlayerPosition Position { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Nationality { get; set; } = string.Empty;
    public double HeightCm { get; set; }
    public double WeightKg { get; set; }

    public Guid TeamId { get; set; }
    public Team Team { get; set; } = null!;

    public ICollection<PlayerStats> Stats { get; set; } = new List<PlayerStats>();
}
