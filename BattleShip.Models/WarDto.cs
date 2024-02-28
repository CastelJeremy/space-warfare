namespace BattleShip.Models;

public class WarDto
{
    public Guid Id { get; set; }
    public WarStatus Status { get; set; }
    public SpacecraftDto[] CommanderFleet { get; set; } = null!;
    public List<Beam> CommanderBeams { get; set; } = new();
    public List<Beam> CosmosBeams { get; set; } = new();
    public string CommanderName { get; set; } = null!;
    public string? CosmosName { get; set; } = null;
}
