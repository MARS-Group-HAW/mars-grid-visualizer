using Godot;
using System.Text.Json;
using J = System.Text.Json.Serialization.JsonPropertyNameAttribute;
using MarsGridVisualizer.LaserTag;

namespace MarsGridVisualizer.Infrastructure;

public abstract record JsonMessages
{
	public sealed record InitialStateMessage : JsonMessages
	{
		[J("currentTick")] public long CurrentTick { get; set; }
		[J("maxTicks")] public long MaxTicks { get; set; }
		[J("worldSize")] public required Dictionary<string, decimal> WorldSize { get; set; }
		[J("rasters")] public required RasterLayer[] Rasters { get; set; }
		[J("vectors")] public required VectorLayer[] Vectors { get; set; }
	}

	public sealed record EntityUpdate : JsonMessages
	{
		[J("currentTick")] public int CurrentTick { get; set; }
		[J("typeName")] public required string TypeName { get; set; }
		[J("t")] public int LayerId { get; set; }
		[J("entities")] public required JsonEntity[] Entities { get; set; }
	}

	public sealed record RasterUpdate : JsonMessages
	{
		[J("currentTick")] public long CurrentTick { get; set; }
		[J("rasters")] public required RasterLayer[] Rasters { get; set; }
	}

	public sealed record VectorUpdate : JsonMessages
	{
		[J("currentTick")] public long CurrentTick { get; set; }
		[J("vectors")] public required VectorLayer[] Vectors { get; set; }
	}
}

public struct JsonEntity
{
	[J("key")] public long Id { get; set; }
	[J("x")] public int X { get; set; }
	[J("y")] public int Y { get; set; }
	[J("b")] public int B { get; set; }
	[J("p")] public EntityProperties? P { get; set; }
}

public struct EntityProperties
{
	// HACK: this is specific to LaserTag right now and should be generic but that
	// probably requires reflection that I first have to think about some more.
	[J("TeamName")] public string? TeamName { get; set; }
}

public struct RasterLayer
{
	[J("typeName")] public string TypeName { get; set; }
	[J("t")] public long LayerId { get; set; }
	[J("cellHeight")] public long CellHeight { get; set; }
	[J("cellWidth")] public long CellWidth { get; set; }
	[J("cells")] public decimal[][] Cells { get; set; }
}

public struct VectorLayer
{
	[J("typeName")] public string TypeName { get; set; }
	[J("t")] public long LayerId { get; set; }
	[J("f")] public Feature[] F { get; set; }
}

public struct Feature
{
	[J("type")] public string Type { get; set; }
	[J("geometry")] public Geometry Geometry { get; set; }
	[J("properties")] public Properties Properties { get; set; }
}

public struct Geometry
{
	[J("type")] public string Type { get; set; }
	[J("coordinates")] public decimal[][][] Coordinates { get; set; }
}

public struct Properties
{
}

/** <summary>
 * Takes a message from a simulation and translates it
 * to structures the visualization understands.
 * </summary>
 */
public static class Adapter
{
	private static readonly JsonSerializerOptions jsonOptions = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		PropertyNameCaseInsensitive = true,
	};

	public static AgentJsonData ModelFrom(string message)
	{
		var parsed = JsonSerializer.Deserialize<AgentJsonData>(message, jsonOptions);
		if (parsed == null)
		{
			GD.PrintErr("could not serialize json to AgentJsonData");
			throw new Exception();
		}

		return parsed;
	}

	public static JsonMessages? ModelFromPythonViz(string message)
	{
		using var doc = JsonDocument.Parse(message);
		var root = doc.RootElement;

		if (root.TryGetProperty("worldSize", out _))
			return JsonSerializer.Deserialize<JsonMessages.InitialStateMessage>(message, jsonOptions);

		if (root.TryGetProperty("entities", out _))
			return JsonSerializer.Deserialize<JsonMessages.EntityUpdate>(message, jsonOptions);

		if (root.TryGetProperty("rasters", out _))
			return JsonSerializer.Deserialize<JsonMessages.RasterUpdate>(message, jsonOptions);

		if (root.TryGetProperty("vectors", out _))
			return JsonSerializer.Deserialize<JsonMessages.VectorUpdate>(message, jsonOptions);

		return null;
	}
}
