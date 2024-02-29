using BattleShip.Models;

public class Astec
{
    private int Size { get; set; }
    private char[,] Grid;
    public Spacecraft[] Fleet { get; } = new Spacecraft[4];

    public Astec(Spacecraft[] fleet, int size)
    {
        Size = size;
        Fleet = fleet.Select(Spacecraft.Clone).ToArray();
        Grid = new char[Size, Size];

        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
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

                int minPosX = spacecraft.Orientation == Orientation.SOUTH ? spacecraft.Size : 0;
                int maxPosX = spacecraft.Orientation == Orientation.NORTH ? Size - spacecraft.Size : Size;
                int minPosY = spacecraft.Orientation == Orientation.EAST ? spacecraft.Size : 0;
                int maxPosY = spacecraft.Orientation == Orientation.WEST ? Size - spacecraft.Size : Size;

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
                        posX++;
                        break;
                    case Orientation.EAST:
                        posY--;
                        break;
                    case Orientation.SOUTH:
                        posX--;
                        break;
                    case Orientation.WEST:
                        posY++;
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
                    posX++;
                    break;
                case Orientation.EAST:
                    posY--;
                    break;
                case Orientation.SOUTH:
                    posX--;
                    break;
                case Orientation.WEST:
                    posY++;
                    break;
            }
        }

        return false;
    }

    public bool MoveSpacecraft(Spacecraft newSpacecraft)
    {
        Spacecraft spacecraft = Fleet.Where((s) => s.Id == newSpacecraft.Id).First();

        // Check new position is free and in boundary
        int posX = newSpacecraft.PosX;
        int posY = newSpacecraft.PosY;

        for (int i = 0; i < newSpacecraft.Size; i++)
        {
            if (posX < 0 || posX > Size - 1 || posY < 0 || posY > Size - 1)
            {
                return false;
            }

            if (Grid[posX, posY] != '\0' && Grid[posX, posY] != newSpacecraft.Id)
            {
                return false;
            }

            switch (newSpacecraft.Orientation)
            {
                case Orientation.NORTH:
                    posX++;
                    break;
                case Orientation.EAST:
                    posY--;
                    break;
                case Orientation.SOUTH:
                    posX--;
                    break;
                case Orientation.WEST:
                    posY++;
                    break;
            }
        }

        // Remove the spacecraft
        posX = spacecraft.PosX;
        posY = spacecraft.PosY;

        for (int i = 0; i < spacecraft.Size; i++)
        {
            Grid[posX, posY] = '\0';

            switch (spacecraft.Orientation)
            {
                case Orientation.NORTH:
                    posX++;
                    break;
                case Orientation.EAST:
                    posY--;
                    break;
                case Orientation.SOUTH:
                    posX--;
                    break;
                case Orientation.WEST:
                    posY++;
                    break;
            }
        }

        // Place the newSpacecraft
        posX = newSpacecraft.PosX;
        posY = newSpacecraft.PosY;

        for (int i = 0; i < newSpacecraft.Size; i++)
        {
            Grid[posX, posY] = newSpacecraft.Id;

            switch (newSpacecraft.Orientation)
            {
                case Orientation.NORTH:
                    posX++;
                    break;
                case Orientation.EAST:
                    posY--;
                    break;
                case Orientation.SOUTH:
                    posX--;
                    break;
                case Orientation.WEST:
                    posY++;
                    break;
            }
        }

        // Copy necessary values "Security"
        spacecraft.PosX = newSpacecraft.PosX;
        spacecraft.PosY = newSpacecraft.PosY;
        spacecraft.Orientation = newSpacecraft.Orientation;

        return true;
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
        char[,] grid = new char[Size, Size];
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                grid[i, j] = Grid[i, j];
            }
        }

        foreach (Beam b in beams)
        {
            grid[b.PosX, b.PosY] = '¤';
        }

        Console.WriteLine();

        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                Console.Write((grid[i, j] == '\0' ? "*" : grid[i, j]) + " ");
            }
            Console.WriteLine();
        }
    }
}

