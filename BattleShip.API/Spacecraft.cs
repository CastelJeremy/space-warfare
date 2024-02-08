using BattleShip.Models;

public class Spacecraft
{
    public char Id { get; set; }
    public int PosX { get; set; }
    public int PosY { get; set; }
    public int Size { get; set; }
    public int Life { get; set; }
    public Orientation Orientation { get; set; }

    public static Spacecraft Clone(Spacecraft spacecraft)
    {
        return new Spacecraft
        {
            Id = spacecraft.Id,
            PosX = spacecraft.PosX,
            PosY = spacecraft.PosY,
            Size = spacecraft.Size,
            Life = spacecraft.Life,
            Orientation = spacecraft.Orientation
        };
    }

    public static SpacecraftDto ToDto(Spacecraft spacecraft)
    {
        return new SpacecraftDto
        {
            Id = spacecraft.Id,
            PosX = spacecraft.PosX,
            PosY = spacecraft.PosY,
            Size = spacecraft.Size,
            Orientation = spacecraft.Orientation
        };
    }
}
