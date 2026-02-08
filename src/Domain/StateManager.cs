namespace MarsGridVisualizer.Domain;

public class StateManager
{
	private readonly SortedDictionary<long, State> ticks = [];
	private long currentTick = 0;

	public void Add(State tick)
	{
		if (ticks.TryGetValue(tick.CurrentTick, out var existing))
		{
			existing.Merge(tick);
		}
		else
		{
			ticks.Add(tick.CurrentTick, tick);
		}
	}

	public State? Current => ticks.GetValueOrDefault(currentTick);
	public State? Next() => ticks.GetValueOrDefault(++currentTick);
	public State? Previous() => ticks.GetValueOrDefault(--currentTick);
}
