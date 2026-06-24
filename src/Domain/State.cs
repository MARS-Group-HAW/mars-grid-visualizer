using System.Diagnostics;
using MarsGridVisualizer.Infrastructure;

namespace MarsGridVisualizer.Domain;

public readonly record struct Entity(long Id, int X, int Y, int Bearing)
{
	static internal Entity FromJsonEntity(JsonEntity e) => new(e.Id, e.X, e.Y, e.B);
}

public class State(int currentTick, Dictionary<string, Entity[]> agentTypes)
{
	public int CurrentTick { get; } = currentTick;
	public Dictionary<string, Entity[]> AgentTypes { get; } = agentTypes;

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
	}

	internal static State FromJsonModel(JsonMessages msg)
	{
		// TODO:
		// JsonMessages.EntityUpdate state = msg switch
		// {
		// 	JsonMessages.InitialStateMessage initial => throw new NotImplementedException(),
		// 	JsonMessages.EntityUpdate update => throw new NotImplementedException(),
		// 	JsonMessages.RasterUpdate update => throw new NotImplementedException(),
		// 	JsonMessages.VectorUpdate update => throw new NotImplementedException(),
		// 	_ => throw new UnreachableException(),
		// };
		//

		var state = (JsonMessages.EntityUpdate)msg;
		var mapped = new Entity[state.Entities.Length];
		for (var i = 0; i < state.Entities.Length; i++)
			mapped[i] = Entity.FromJsonEntity(state.Entities[i]);

		return new(state.CurrentTick, new Dictionary<string, Entity[]>
		{
			[state.TypeName] = mapped,
		});
	}
}
