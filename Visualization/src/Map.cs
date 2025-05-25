using Godot;

namespace mmvp.src;

public class Map(List<List<Map.Field>> data)
{
    private readonly Vector2I? wall = new(12, 5);
    private readonly Vector2I? wallCorner = new(12, 4);
    private readonly Vector2I? floor = null;
    private readonly Vector2I? hill = new(6, 0);
    private readonly Vector2I? ditch = new(7, 4);
    private readonly Vector2I? water = new(7, 4);
    private readonly Vector2I? explosiveBarrel = new(7, 4);
    private readonly Vector2I? flagStand1 = new(7, 4);
    private readonly Vector2I? flagStand2 = new(7, 4);

    public enum Field
    {
        Floor,
        Wall,
        Hill,
        Ditch,
        Water,
        ExplosiveBarrel,
        FlagStand1,
        FlagStand2,
    }

    private readonly List<List<Field>> data = data;

    public static Map ReadInMap(string filename)
    {
        var mapData = new List<List<Field>>();

        string[] lines = File.ReadAllLines(filename);

        foreach (string line in lines)
        {
            var row = line
                .Split(';')
                .Select(field => field switch
            {
                "0" => Field.Floor,
                "1" => Field.Wall,
                "2" => Field.Hill,
                "3" => Field.Ditch,
                "4" => Field.Water,
                "5" => Field.ExplosiveBarrel,
                "7" => Field.FlagStand1,
                "8" => Field.FlagStand2,
                _ => throw new ArgumentException("Encountered an unknown map field."),
            })
            .ToList();
            mapData.Add(row);
        }
        return new Map(mapData);
    }

    public override string ToString()
    {
        return string.Join("",
                data.ConvertAll(row => string.Join("",
                        row.ConvertAll(field => field switch
                        {
                            Field.Floor => " ",
                            Field.Wall => "H",
                            Field.Hill => "^",
                            Field.Ditch => "_",
                            _ => "="
                        }))
                    + "\n"));
    }

    public void PopulateTileMap(TileMapLayer tileMapLayer)
    {
        for (int y = 0; y < data.Count; ++y)
        {
            for (int x = 0; x < data[y].Count; ++x)
            {
                var fieldValue = data[y][x];
                Vector2I? atlasCoords = fieldValue switch
                {
                    Field.Floor => floor,
                    Field.Wall => wall,
                    Field.Hill => hill,
                    Field.Ditch => ditch,
                    Field.Water => water,
                    Field.ExplosiveBarrel => explosiveBarrel,
                    Field.FlagStand1 => flagStand1,
                    Field.FlagStand2 => flagStand2,
                    _ => throw new NotImplementedException(),
                };
                tileMapLayer.SetCell(new Vector2I(x, y), 3, atlasCoords);
            }
        }
    }

    enum WallPosition
    {
        Horizontal,
        Vertical,
        LeftUpperCorner,
        RightUpperCorner,
        LeftLowerCorner,
        RightLowerCorner,
    }

    private WallPosition GetWallPosition(int x, int y)
    {
        bool hasNorth = HasWall(x, y - 1);
        bool hasSouth = HasWall(x, y + 1);
        bool hasEast = HasWall(x + 1, y);
        bool hasWest = HasWall(x - 1, y);

        int wallCount = (hasNorth ? 1 : 0) + (hasSouth ? 1 : 0) +
                        (hasEast ? 1 : 0) + (hasWest ? 1 : 0);

        return wallCount switch
        {
            1 => WallPosition.Horizontal,
            2 when hasNorth && hasEast => WallPosition.LeftUpperCorner,
            2 when hasNorth && hasEast => WallPosition.RightUpperCorner,
            2 when hasNorth && hasEast => WallPosition.LeftLowerCorner,
            2 when hasNorth && hasEast => WallPosition.RightLowerCorner,
            _ => WallPosition.Horizontal,

        };
    }

    private bool HasWall(int x, int y)
    {
        // handle out of bounds
        if (y < 0 || y >= data.Count || x < 0 || x >= data[y].Count)
            return false;

        return data[y][x] == Field.Wall;
    }
}

