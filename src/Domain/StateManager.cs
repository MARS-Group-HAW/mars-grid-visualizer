namespace MarsGridVisualizer.Domain;

public class StateManager
{
	private readonly SortedDictionary<long, State> ticks = [];
	private long currentTick = -1;

	public long CurrentTick => currentTick;
	public long LatestTick { get; private set; } = -1;
	public long EarliestTick { get; private set; } = -1;
	public bool HasAny => ticks.Count > 0;
	public bool IsAtLatest => HasAny && currentTick >= LatestTick;

	public long? MaxTicks { get; set; }
	public Dictionary<string, decimal>? WorldSize { get; set; }

	public void Add(State tick)
	{
		if (ticks.TryGetValue(tick.CurrentTick, out var existing))
		{
			existing.Merge(tick);
			return;
		}

		ticks.Add(tick.CurrentTick, tick);
		if (tick.CurrentTick > LatestTick) LatestTick = tick.CurrentTick;
		if (EarliestTick < 0 || tick.CurrentTick < EarliestTick) EarliestTick = tick.CurrentTick;
	}

	public State? Current => ticks.GetValueOrDefault(currentTick);

	public bool TryAdvance()
	{
		if (ticks.ContainsKey(currentTick + 1))
		{
			currentTick++;
			return true;
		}
		foreach (var key in ticks.Keys)
		{
			if (key > currentTick)
			{
				currentTick = key;
				return true;
			}
		}
		return false;
	}

	public bool TryStepBack()
	{
		if (ticks.ContainsKey(currentTick - 1))
		{
			currentTick--;
			return true;
		}
		long? candidate = null;
		foreach (var key in ticks.Keys)
		{
			if (key >= currentTick) break;
			candidate = key;
		}
		if (candidate is not { } target) return false;
		currentTick = target;
		return true;
	}

	public bool Seek(long tick)
	{
		if (!ticks.ContainsKey(tick)) return false;
		currentTick = tick;
		return true;
	}
}
