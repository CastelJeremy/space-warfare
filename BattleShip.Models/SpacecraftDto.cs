namespace BattleShip.Models;

public class SpacecraftDto
{
    public char Id { get; set; }
    public int PosX { get; set; }
    public int PosY { get; set; }
    public int Size { get; set; }
    public Orientation Orientation { get; set; }
}
