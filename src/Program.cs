using Chickensoft.GameTools.Displays;
using Godot;
using MarsGridVisualizer.Domain;
using MarsGridVisualizer.Infrastructure;
using MarsGridVisualizer.Presentation;
using MarsGridVisualizer.Ui;

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
	private BaseMapLayer? tileMapLayer;
	private readonly StateManager store = new();
	private GameState gameState = new GameState.Loading();
	private Godot2DRenderer renderer = new();
	private readonly bool useLaserTagApi = false;

	private ConnectionBadge badge = null!;
	private PlayButton playButton = null!;
	private HSlider scrubber = null!;
	private Button jumpToLatestButton = null!;
	private Button stepBackButton = null!;
	private Button stepForwardButton = null!;
	private bool isPaused = false;
	private bool suppressScrubberSignal = false;

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
			return;
		}

		badge = GetNode<ConnectionBadge>("%ConnectionBadge");
		playButton = GetNode<PlayButton>("%PlayButton");
		scrubber = GetNode<HSlider>("%Scrubber");
		jumpToLatestButton = GetNode<Button>("%JumpToLatestButton");
		stepBackButton = GetNode<Button>("%StepBackButton");
		stepForwardButton = GetNode<Button>("%StepForwardButton");

		playButton.PausedChanged += paused => isPaused = paused;
		scrubber.ValueChanged += OnScrubberChanged;
		jumpToLatestButton.Pressed += OnJumpToLatestPressed;
		stepBackButton.Pressed += () => StepWithPause(forward: false);
		stepForwardButton.Pressed += () => StepWithPause(forward: true);

		client.OnConnected += () => badge.SetState(ConnectionState.Connected);
		client.OnDisconnected += (int closeCode, string closeReason) =>
		{
			if (gameState is GameState.Finished) return;
			badge.SetState(ConnectionState.Disconnected);
		};

		client.OnMessage += message =>
		{
			var msg = Adapter.ModelFromPythonViz(message);
			if (msg is null) return;
			var state = State.FromJsonModel(msg);
			store.Add(state);
			RefreshScrubberBounds();
			RefreshJumpButton();
		};
		client.Connect("ws://127.0.0.1:4567/vis");

		var timer = new Godot.Timer { WaitTime = 0.1, Autostart = true };
		timer.Timeout += () =>
		{
			if (isPaused) return;
			if (!store.TryAdvance()) return;
			RenderCurrent();
			SyncScrubberToCursor();
			RefreshJumpButton();
		};
		AddChild(timer);
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
			return;
		}

		if (useLaserTagApi) return;

		if (key.IsActionPressed("playback_toggle"))
		{
			playButton.ButtonPressed = !playButton.ButtonPressed;
		}
		else if (key.IsActionPressed("playback_step_forward"))
		{
			StepWithPause(forward: true);
		}
		else if (key.IsActionPressed("playback_step_back"))
		{
			StepWithPause(forward: false);
		}
		else if (key.IsActionPressed("playback_jump_latest"))
		{
			OnJumpToLatestPressed();
		}
	}

	private void OnScrubberChanged(double value)
	{
		if (suppressScrubberSignal) return;
		if (!store.Seek((long)value)) return;
		RenderCurrent();
		RefreshJumpButton();
	}

	private void OnJumpToLatestPressed()
	{
		if (!store.HasAny) return;
		if (!store.Seek(store.LatestTick)) return;
		RenderCurrent();
		SyncScrubberToCursor();
		RefreshJumpButton();
	}

	private void StepWithPause(bool forward)
	{
		if (!isPaused) playButton.ButtonPressed = true;
		var stepped = forward ? store.TryAdvance() : store.TryStepBack();
		if (!stepped) return;
		RenderCurrent();
		SyncScrubberToCursor();
		RefreshJumpButton();
	}

	private void RenderCurrent()
	{
		if (store.Current is not { } current) return;
		renderer.Render(new State(current.CurrentTick, current.AgentTypes));
	}

	private void SyncScrubberToCursor()
	{
		if (store.CurrentTick < 0) return;
		suppressScrubberSignal = true;
		scrubber.Value = store.CurrentTick;
		suppressScrubberSignal = false;
	}

	private void RefreshScrubberBounds()
	{
		if (!store.HasAny) return;
		suppressScrubberSignal = true;
		scrubber.MaxValue = store.LatestTick;
		suppressScrubberSignal = false;
	}

	private void RefreshJumpButton()
	{
		jumpToLatestButton.Disabled = !store.HasAny || store.IsAtLatest;
	}
}
