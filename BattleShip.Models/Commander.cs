namespace BattleShip.Models;

public class Commander
{
    public Guid Id { get; set; }
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
}
