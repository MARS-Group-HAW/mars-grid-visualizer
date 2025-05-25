using System.Diagnostics;
using Godot;

namespace mmvp.src;


public class Map(List<List<Map.Field>> data)
{
    private static TileSetCoordinates GetScribbleDungeonTiles()
    {

        return new TileSetCoordinates(
            Wall: new(new(5, 5)),
            WallCorner: new(new(5, 4)),
            Floor: new(new(0, 0)),
            Hill: new(new(5, 0)),
            Ditch: new(new(0, 0), 1),
            Water: new(new(0, 0), 2),
            ExplosiveBarrel: new(new(5, 2)),
            FlagStand1: new(new(1, 8)),
            FlagStand2: new(new(3, 8))
        );
    }

    private record TileSetCoordinates(
        TileMapCell Wall,
        TileMapCell WallCorner,
        TileMapCell Floor,
        TileMapCell Hill,
        TileMapCell Ditch,
        TileMapCell Water,
        TileMapCell ExplosiveBarrel,
        TileMapCell FlagStand1,
        TileMapCell FlagStand2);

    private record TileMapCell(Vector2I AtlasCoords, int AlternativeTile = 0);

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

    private readonly TileSetCoordinates currentTileSetCoords = GetScribbleDungeonTiles();

    public static Map ReadInMap(string filename)
    {
        var mapData = new List<List<Field>>();
        string[] lines = File.ReadAllLines(filename);
        foreach (string line in lines)
        {
            var row = line.Split(';').Select(field => field switch
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
            }).ToList();
            mapData.Add(row);
        }

        return new Map(mapData);
    }

    public override string ToString()
    {
        return string.Join("", data.ConvertAll(row => string.Join("", row.ConvertAll(field => field switch
        {
            Field.Floor => " ",
            Field.Wall => "H",
            Field.Hill => "^",
            Field.Ditch => "_",
            _ => "="
        })) + "\n"));
    }

    public void PopulateTileMap(TileMapLayer tileMapLayer)
    {
        for (int y = 0; y < data.Count; ++y)
        {
            for (int x = 0; x < data[y].Count; ++x)
            {
                var fieldValue = data[y][x];
                var tileSetField = fieldValue switch
                {
                    Field.Floor => currentTileSetCoords.Floor,
                    Field.Wall => currentTileSetCoords.Wall,
                    Field.Hill => currentTileSetCoords.Hill,
                    Field.Ditch => currentTileSetCoords.Ditch,
                    Field.Water => currentTileSetCoords.Water,
                    Field.ExplosiveBarrel => currentTileSetCoords.ExplosiveBarrel,
                    Field.FlagStand1 => currentTileSetCoords.FlagStand1,
                    Field.FlagStand2 => currentTileSetCoords.FlagStand2,
                    _ => throw new NotImplementedException(),
                };
                if (fieldValue is not Field.Wall)
                    tileMapLayer.SetCell(new Vector2I(x, y), 3, tileSetField.AtlasCoords);
                else
                    switch (GetWallPosition(x, y))
                    {
                        case WallPosition.Vertical:
                            tileMapLayer.SetCell(new Vector2I(x, y), 3, tileSetField.AtlasCoords);
                            break;
                        case WallPosition.Horizontal:
                            tileMapLayer.SetCell(new Vector2I(x, y), 3, tileSetField.AtlasCoords, 1);
                            break;
                        case WallPosition.LeftUpperCorner:
                            tileMapLayer.SetCell(new Vector2I(x, y), 3, currentTileSetCoords.WallCorner.AtlasCoords);
                            break;
                        case WallPosition.RightUpperCorner:
                            tileMapLayer.SetCell(new Vector2I(x, y), 3, currentTileSetCoords.WallCorner.AtlasCoords, 1);
                            break;
                        case WallPosition.LeftLowerCorner:
                            tileMapLayer.SetCell(new Vector2I(x, y), 3, currentTileSetCoords.WallCorner.AtlasCoords, 2);
                            break;
                        case WallPosition.RightLowerCorner:
                            tileMapLayer.SetCell(new Vector2I(x, y), 3, currentTileSetCoords.WallCorner.AtlasCoords, 3);
                            break;
                        default:
                            throw new UnreachableException();
                    }
            }
        }
    }

    private enum WallPosition
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
            2 when hasNorth && hasSouth => WallPosition.Vertical,
            2 when hasSouth && hasEast => WallPosition.LeftUpperCorner,
            2 when hasSouth && hasWest => WallPosition.RightUpperCorner,
            2 when hasNorth && hasEast => WallPosition.LeftLowerCorner,
            2 when hasNorth && hasWest => WallPosition.RightLowerCorner,
            _ => WallPosition.Horizontal,

        };
    }

    private bool HasWall(int x, int y)
    {
        // handle out of bounds
        if (y < 0 || y >= data.Count || x < 0 || x >= data[y].Count)
            return false;

        return data[y][x] is Field.Wall;
    }
}

