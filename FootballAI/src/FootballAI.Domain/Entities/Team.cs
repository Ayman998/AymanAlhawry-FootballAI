using FootballAI.src.FootballAI.Domain.Common;

namespace FootballAI.src.FootballAI.Domain.Entities;

public class Team : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string PrimaryColorHex { get; set; } = "#FFFFFF";
    public string SecondaryColorHex { get; set; } = "#000000";

    public ICollection<Player> Players { get; set; } = new List<Player>();
}
