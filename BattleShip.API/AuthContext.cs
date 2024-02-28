using BattleShip.Models;
using Microsoft.EntityFrameworkCore;

public class AuthContext : DbContext
{
    public DbSet<Commander> Commanders { get; set; } = null!;
    public DbSet<Session> Sessions { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql(@"Host=localhost;Username=postgres;Password=admin;Database=space-warfare");
}
