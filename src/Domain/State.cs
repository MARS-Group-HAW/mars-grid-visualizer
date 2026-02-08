namespace MarsGridVisualizer.Domain;

public record Entity(long Id, int X, int Y, int Bearing);

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
}
