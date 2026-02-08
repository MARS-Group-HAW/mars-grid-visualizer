using Godot;

namespace MarsGridVisualizer;


public class Map(List<List<Map.Field>> data)
{
	private const int wallTerrain = 0;

	private static TileSetCoordinates GetScribbleDungeonTiles()
	{

		return new TileSetCoordinates(
			SourceId: 3,
			Floor: new(new(0, 0)),
			Hill: new(new(5, 0)),
			Ditch: new(new(0, 0), 1),
			Water: new(new(0, 0), 2),
			ExplosiveBarrel: new(new(5, 2)),
			FlagStand1: new(new(1, 8)),
			FlagStand2: new(new(3, 8))
		);
	}

	private static TileSetCoordinates GetMinipackTiles()
	{

		return new TileSetCoordinates(
			SourceId: 0,
			Floor: new(new(4, 2)),
			Hill: new(new(5, 0)),
			Ditch: new(new(4, 2), 1),
			Water: new(new(0, 3)),
			ExplosiveBarrel: new(new(3, 2)),
			FlagStand1: new(new(1, 8)),
			FlagStand2: new(new(3, 8))
		);
	}

	private static TileSetCoordinates GetTopDownShooterTiles()
	{

		return new TileSetCoordinates(
			SourceId: 2,
			Floor: new(new(22, 14)),
			Hill: new(new(26, 18)),
			Ditch: new(new(26, 17)),
			Water: new(new(25, 18)),
			ExplosiveBarrel: new(new(25, 16)),
			FlagStand1: new(new(26, 1)),
			FlagStand2: new(new(26, 1))
		);
	}

	private record TileSetCoordinates(
		int SourceId,
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
		FlagStandRed,
		FlagStandYellow,
	}

	private TileSetCoordinates currentTileSetCoords = GetMinipackTiles();

	public static Map ReadInMap(string mapPath)
	{
		var lines = File.ReadAllLines(mapPath);
		return ReadInMapFromLines(lines);
	}

	public static Map ReadInMapFromLines(string[] lines)
	{
		var mapData = lines.Select(line => line.Split(';')
				.Select(field => field switch
				{
					"0" => Field.Floor,
					"1" => Field.Wall,
					"2" => Field.Hill,
					"3" => Field.Ditch,
					"4" => Field.Water,
					"5" => Field.ExplosiveBarrel,
					"7" => Field.FlagStandRed,
					"8" => Field.FlagStandYellow,
					var any => throw new ArgumentException($"Encountered an unknown map field: '{any}'"),
				})
				.ToList())
			.ToList();

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

	public Vector2I Size()
	{
		return new(data[0].Count, data.Count);
	}

	public void PopulateTileMap(TileMapLayer tileMapLayer)
	{
		currentTileSetCoords = tileMapLayer.Name.ToString() switch
		{
			"BaseMap" => GetScribbleDungeonTiles(),
			"MinipackBaseMap" => GetMinipackTiles(),
			"TopDownShooterBaseMap" => GetTopDownShooterTiles(),
			var other => throw new NotImplementedException($"No TileSet for: {other}"),
		};

		var wallCells = new Godot.Collections.Array<Vector2I>();

		for (int y = 0; y < data.Count; ++y)
		{
			for (int x = 0; x < data[y].Count; ++x)
			{
				var fieldValue = data[y][x];

				if (fieldValue == Field.Wall)
				{
					wallCells.Add(new Vector2I(x, y));
					continue;
				}

				var tileSetField = fieldValue switch
				{
					Field.Floor => currentTileSetCoords.Floor,
					Field.Hill => currentTileSetCoords.Hill,
					Field.Ditch => currentTileSetCoords.Ditch,
					Field.Water => currentTileSetCoords.Water,
					Field.ExplosiveBarrel => currentTileSetCoords.ExplosiveBarrel,
					Field.FlagStandRed => currentTileSetCoords.FlagStand1,
					Field.FlagStandYellow => currentTileSetCoords.FlagStand2,
					_ => throw new NotImplementedException(),
				};

				tileMapLayer.SetCell(
					new Vector2I(x, y),
					currentTileSetCoords.SourceId,
					tileSetField.AtlasCoords,
					tileSetField.AlternativeTile
				);
			}
		}

		tileMapLayer.SetCellsTerrainConnect(wallCells, terrainSet: 0, terrain: wallTerrain);
	}

}
