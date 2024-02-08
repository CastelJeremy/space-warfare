using BattleShip.Models;

public class Astec
{
    private char[,] Grid = new char[10, 10];
    public Spacecraft[] Fleet { get; } = new Spacecraft[4];

    public Astec(Spacecraft[] fleet)
    {
        Fleet = fleet.Select(Spacecraft.Clone).ToArray();

        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                Grid[i, j] = '\0';
            }
        }

        Array orientationValues = Enum.GetValues(typeof(Orientation));

        foreach (Spacecraft spacecraft in Fleet)
        {
            do
            {
                spacecraft.Orientation = (Orientation)orientationValues.GetValue(Random.Shared.Next(orientationValues.Length))!;

                int minPosX = spacecraft.Orientation == Orientation.EAST ? spacecraft.Size : 0;
                int maxPosX = spacecraft.Orientation == Orientation.WEST ? 10 - spacecraft.Size : 10;
                int minPosY = spacecraft.Orientation == Orientation.SOUTH ? spacecraft.Size : 0;
                int maxPosY = spacecraft.Orientation == Orientation.NORTH ? 10 - spacecraft.Size : 10;

                spacecraft.PosX = Random.Shared.Next(minPosX, maxPosX);
                spacecraft.PosY = Random.Shared.Next(minPosY, maxPosY);
            } while (Intersect(spacecraft));

            int posX = spacecraft.PosX;
            int posY = spacecraft.PosY;
            for (int j = 0; j < spacecraft.Size; j++)
            {
                Grid[posX, posY] = spacecraft.Id;

                switch (spacecraft.Orientation)
                {
                    case Orientation.NORTH:
                        posY++;
                        break;
                    case Orientation.EAST:
                        posX--;
                        break;
                    case Orientation.SOUTH:
                        posY--;
                        break;
                    case Orientation.WEST:
                        posX++;
                        break;
                }
            }
        }
    }

    private bool Intersect(Spacecraft spacecraft)
    {
        int posX = spacecraft.PosX;
        int posY = spacecraft.PosY;

        for (int i = 0; i < spacecraft.Size; i++)
        {
            if (Grid[posX, posY] != '\0')
            {
                return true;
            }

            switch (spacecraft.Orientation)
            {
                case Orientation.NORTH:
                    posY++;
                    break;
                case Orientation.EAST:
                    posX--;
                    break;
                case Orientation.SOUTH:
                    posY--;
                    break;
                case Orientation.WEST:
                    posX++;
                    break;
            }
        }

        return false;
    }

    public bool Hit(int x, int y)
    {
        if (Grid[x, y] == '\0')
        {
            return false;
        }

        char spacecraftId = Grid[x, y];
        Spacecraft spacecraft = Fleet.Where(s => s.Id == spacecraftId).First();
        spacecraft.Life--;

        return true;
    }

    public void Dump(List<Beam> beams)
    {
        char[,] grid = new char[10, 10];
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                grid[i, j] = Grid[i, j];
            }
        }

        foreach (Beam b in beams)
        {
            grid[b.PosX, b.PosY] = '¤';
        }

        Console.WriteLine();

        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                Console.Write((grid[i, j] == '\0' ? "*" : grid[i, j]) + " ");
            }
            Console.WriteLine();
        }
    }
}

