using BattleShip.Models;

public class AstecCellModel
{
    public SpacecraftPart? SpacecraftPart { get; set; } = null;
    public Orientation? SpacecraftOrientation { get; set; } = null;
    public bool Empty { get; set; } = true;
    public bool Beam { get; set; } = false;
    public bool Hit { get; set; } = false;
}
