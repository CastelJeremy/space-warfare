namespace BattleShip.Models;

public class BeamResponseDto
{
    public WarStatus Status { get; set; }
    public string Winner { get; set; } = null!;
    public bool Hit { get; set; }
    public Beam? CosmosBeam { get; set; }
}
