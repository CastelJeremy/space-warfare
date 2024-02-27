namespace BattleShip.Models;

public class Session
{
    public Guid Id { get; set; }
    public Guid CommanderId { get; set; }
    public virtual Commander Commander { get; set; } = null!;
    public DateTime CreationDate { get; set; }
    public string Ip { get; set; } = null!;
    public string Fingerprint { get; set; } = null!;
}
