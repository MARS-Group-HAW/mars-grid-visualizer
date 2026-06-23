using Chickensoft.GameTools.Displays;
using Godot;
using MarsGridVisualizer.Domain;
using MarsGridVisualizer.Infrastructure;
using MarsGridVisualizer.Presentation;

namespace MarsGridVisualizer;

public abstract record GameState
{
	public sealed record Loading : GameState;
	public sealed record Playing : GameState;
	public sealed record Paused : GameState;
	public sealed record Finished : GameState;
}

public partial class Program : Control
{
	private readonly WebSocketClient client = new();
	private Label? waitingLabel;
	private BaseMapLayer? tileMapLayer;
	private readonly StateManager store = new();
	private GameState gameState = new GameState.Loading();
	private Godot2DRenderer renderer = new();
	private readonly bool useLaserTagApi = false;

	public override void _Ready()
	{
		GetWindow().LookGood(
			WindowScaleBehavior.UIFixed,
			themeResolution: Display.QHD,
			maxWindowedSize: 1.0f);

		AddChild(renderer);

		tileMapLayer = GetNode<BaseMapLayer>("%TopDownShooterBaseMap");
		renderer.TileMapLayer = tileMapLayer;

		if (useLaserTagApi)
		{
			AddChild(new LaserTag.LaserTag(tileMapLayer));
		}
		else
		{
			client.OnMessage += message =>
			{
				// - TODO: pull for /progress info and merge into one state
				//   - probably good if there is an extra class
				// - TODO: investigate how /progress looks like when CSV or DB is
				//   the source
				var model =
					adapter.ModelFromPythonViz(message)
					?? throw new NotImplementedException("model is null");
				store.Add(State.FromJsonModel(model));
			};
			client.Connect("ws://127.0.0.1:4567/vis");

			var timer = new Godot.Timer { WaitTime = 0.1, Autostart = true };
			timer.Timeout += () =>
			{
				var tick = store.Next();
				if (tick is { } model)
					renderer.Render(new State(tick.CurrentTick, tick.AgentTypes));
			};
			AddChild(timer);
		}
	}

	public override void _Process(double delta)
	{
		if (useLaserTagApi) return;
		client.Next(delta);
	}

	public override void _Input(InputEvent @event)
	{
		if (!IsInstanceValid(@event)) return;
		if (@event is not InputEventKey key) return;

		if (key.IsActionPressed("quit"))
		{
			GetTree().CallDeferred("quit");
		}
	}
}
