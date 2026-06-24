using System.Diagnostics;
using MarsGridVisualizer.Infrastructure;

namespace MarsGridVisualizer.Domain;

public readonly record struct Entity(long Id, int X, int Y, int Bearing)
{
	static internal Entity FromJsonEntity(JsonEntity e) => new(e.Id, e.X, e.Y, e.B);
}

public class State(
	long currentTick,
	Dictionary<string, Entity[]> agentTypes,
	Dictionary<long, RasterLayer> rasters,
	Dictionary<long, VectorLayer> vectors)
{
	public long CurrentTick { get; } = currentTick;
	public Dictionary<string, Entity[]> AgentTypes { get; } = agentTypes;
	public Dictionary<long, RasterLayer> Rasters { get; } = rasters;
	public Dictionary<long, VectorLayer> Vectors { get; } = vectors;

	public void Merge(State other)
	{
		if (CurrentTick != other.CurrentTick)
			throw new ArgumentException(
				$"this tick {CurrentTick} and {other.CurrentTick} are part of the same tick"
			);

		foreach (var key in other.AgentTypes.Keys)
			// NOTE: For some reason this was throwing but I can't reproduce it now.
			//       I still think this should hold true, unless the server sends
			//       duplicate, potentially dropped messages.
			if (AgentTypes.ContainsKey(key))
				throw new ArgumentException("Received duplicate Entities");
			else
				AgentTypes[key] = other.AgentTypes[key];

		foreach (var (key, value) in other.Rasters)
			if (!Rasters.TryAdd(key, value))
				throw new ArgumentException($"Received duplicate Raster layer {key}");

		foreach (var (key, value) in other.Vectors)
			if (!Vectors.TryAdd(key, value))
				throw new ArgumentException($"Received duplicate Vector layer {key}");
	}

	internal static State FromJsonModel(JsonMessages msg) => msg switch
	{
		JsonMessages.InitialStateMessage m => new(
			m.CurrentTick,
			[],
			m.Rasters.ToDictionary(r => r.LayerId),
			m.Vectors.ToDictionary(v => v.LayerId)
		),

		JsonMessages.EntityUpdate m => new(
			m.CurrentTick,
			new Dictionary<string, Entity[]> { [m.TypeName] = MapEntities(m.Entities) },
			[],
			[]
		),

		JsonMessages.RasterUpdate m => new(
			m.CurrentTick,
			[],
			m.Rasters.ToDictionary(r => r.LayerId),
			[]
		),

		JsonMessages.VectorUpdate m => new(
			m.CurrentTick,
			[],
			[],
			m.Vectors.ToDictionary(v => v.LayerId)
		),

		_ => throw new UnreachableException(),
	};

	private static Entity[] MapEntities(JsonEntity[] entities)
	{
		var mapped = new Entity[entities.Length];
		for (var i = 0; i < entities.Length; i++)
			mapped[i] = Entity.FromJsonEntity(entities[i]);
		return mapped;
	}
}
