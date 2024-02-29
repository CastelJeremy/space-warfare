using BattleShip.Models;

public class War
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid CommanderId { get; set; }
    public Guid? CosmosId { get; set; } = null;
    public WarStatus Status { get; set; } = WarStatus.LOBBY;
    private Spacecraft[] Fleet { get; init; } = new Spacecraft[4];
    public int AstecSize { get; set; } = 10;
    public Astec CommanderAstec { get; set; }
    public Astec CosmosAstec { get; set; }
    public List<Beam> CommanderBeams { get; set; } = new();
    public List<Beam> CosmosBeams { get; set; } = new();
    public bool CommanderReady { get; set; } = false;
    public bool CosmosReady { get; set; } = false;

    public War(Guid commanderId)
    {
        CommanderId = commanderId;

        char[] ids = { 'A', 'B', 'C', 'D' };

        for (int i = 0; i < 4; i++)
        {
            int size = Random.Shared.Next(4) + 1;
            Spacecraft spacecraft = new Spacecraft
            {
                Id = ids[i],
                Size = size,
                Life = size
            };
            Fleet[i] = spacecraft;
        }

        CommanderAstec = new Astec(Fleet, AstecSize);
        CosmosAstec = new Astec(Fleet, AstecSize);
    }

    private void CheckWarStatus()
    {
        bool isCosmosDead = CosmosAstec.Fleet.All((s) => s.Life == 0);
        bool isCommanderDead = CommanderAstec.Fleet.All((s) => s.Life == 0);

        if (isCosmosDead || isCommanderDead)
        {
            Status = WarStatus.ENDED;
        }
    }

    public BeamResponseDto Beam(BeamActionDto beam)
    {
        if (Status == WarStatus.ENDED || Status == WarStatus.LOBBY)
        {
            throw new Exception("Can't beam during peace.");
        }

        if (CommanderBeams.Find(b => b.PosX == beam.PosX && b.PosY == beam.PosY) is not null)
        {
            throw new Exception("Can't beam the same spot twice.");
        }

        bool commanderHit = CosmosAstec.Hit(beam.PosX, beam.PosY);
        CommanderBeams.Add(new Beam { PosX = beam.PosX, PosY = beam.PosY, Hit = commanderHit });

        string winner = string.Empty;
        Beam? futureBeam = null;

        CheckWarStatus();
        if (Status == WarStatus.ENDED)
        {
            winner = "Commander";
        }
        else
        {
            List<BeamActionDto> availableBeams = new();
            for (int i = 0; i < AstecSize; i++)
            {
                for (int j = 0; j < AstecSize; j++)
                {
                    availableBeams.Add(new BeamActionDto { PosX = i, PosY = j });
                }
            }

            foreach (Beam previousBeam in CosmosBeams)
            {
                availableBeams.RemoveAll(b => b.PosX == previousBeam.PosX && b.PosY == previousBeam.PosY);
            }

            BeamActionDto selectedBeam = availableBeams[Random.Shared.Next(availableBeams.Count())];
            bool cosmosHit = CommanderAstec.Hit(selectedBeam.PosX, selectedBeam.PosY);
            futureBeam = new Beam { PosX = selectedBeam.PosX, PosY = selectedBeam.PosY, Hit = cosmosHit };

            CosmosBeams.Add(futureBeam);

            CheckWarStatus();
            if (Status == WarStatus.ENDED)
            {
                winner = "Cosmos";
            }
        }

        CosmosAstec.Dump(CommanderBeams);

        return new BeamResponseDto
        {
            Status = Status,
            Winner = winner,
            Hit = commanderHit,
            CosmosBeam = futureBeam
        };
    }

    public WarDto ToDto()
    {
        CommanderAstec.Dump(CosmosBeams);

        return new WarDto
        {
            Id = Id,
            Status = Status,
            CommanderFleet = CommanderAstec.Fleet.Select(Spacecraft.ToDto).ToArray(),
            AstecSize = AstecSize,
            CommanderBeams = CommanderBeams,
            CosmosBeams = CosmosBeams,
            CommanderName = CommanderId.ToString(),
            CosmosName = CosmosId.HasValue ? CosmosId.ToString() : null
        };
    }
}
