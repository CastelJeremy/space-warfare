
namespace BattleShip.Models;

public class SessionDto
{
    public Guid Id { get; set; }
    public DateTime CreationDate { get; set; }
    public string Ip { get; set; } = null!;
    public string Fingerprint { get; set; } = null!;
}
