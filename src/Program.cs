using System.Diagnostics;
using Chickensoft.GameTools.Displays;
using Godot;
using MarsGridVisualizer.Agents;
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
	private const string WEB_SOCKET_URL = "ws://127.0.0.1:8181";
	private const int BarrelRadius = 3;
	private const float AgentMoveDuration = 0.10f;

	private PackedScene agentScene = GD.Load<PackedScene>("res://src/Domain/Agent/agent.tscn");

	private readonly WebSocketClient client = new();
	private int currentTick = 1;
	private TileMapLayer? tileMapLayer;
	private TileSetAtlasSource? tileSetSpritesheet;
	private Map? map;
	private readonly List<AgentJsonData> jsonDataHistory = [];
	private GameState gameState = new GameState.Loading();

	public override void _Ready()
	{
		GetWindow().LookGood(
			WindowScaleBehavior.UIFixed,
			themeResolution: Display.QHD,
			maxWindowedSize: 1.0f);

		tileMapLayer = GetNode<TileMapLayer>("%TopDownShooterBaseMap");
		tileSetSpritesheet = (TileSetAtlasSource)tileMapLayer.TileSet.GetSource(tileMapLayer.TileSet.GetSourceId(0));

		var playButton = GetNode<PlayButton>("LayoutRoot/Timeline/PlayButton");
		playButton.PausedChanged += OnPausedChanged;

		client.OnMessage += model =>
		{
			jsonDataHistory.Add(model);

			if (gameState is GameState.Loading) gameState = new GameState.Playing();
			if (gameState is GameState.Paused or GameState.Finished) return;

			if (map is null)
			{
				map = Map.ReadInMap(model.MapPath);
				map.PopulateTileMap(tileMapLayer);
			}

			UpdateCurrentTick(model.ExpectingTick);
			UpdateScores(model.Scores);
			DrawGame(model);
			// FIXME: since we don't have access to the config anymore, there needs to be another way to
			//        tell, wether the game has ended
			//        - probably best if we have access to the config afterall then
			//          we know where the project root is and can in the future maybe
			//          do other things as well
			client.Send(currentTick.ToString());
			currentTick += 1;
			if (gameState is GameState.Finished) ShowScores();
		};
		client.Connect(WEB_SOCKET_URL);
	}

	private void OnPausedChanged(bool isPaused)
	{
		gameState = isPaused ? new GameState.Paused() : new GameState.Playing();
		if (!isPaused)
		{
			client.Send(currentTick.ToString());
		}
	}

	public override void _Process(double delta)
	{
		client.Next();
	}

	private void ShowScores()
	{
		static string title(string s)
		{
			return $"[font_size=100][center]{s}[/center][/font_size]";
		}

		var endScoreLabel = GetNode<RichTextLabel>("%EndScoreLabel");

		var scores = jsonDataHistory.Last().Scores;

		var isDraw = scores[0].TeamScore == scores[1].TeamScore;
		if (isDraw)
		{
			endScoreLabel.Text = title("Game resulted in a draw!");
			endScoreLabel.Show();
			return;
		}

		var winner = scores.OrderByDescending(x => x.TeamScore).First();
		var c = winner.TeamColor.ColorToHtml();

		endScoreLabel.Text = $"[color={c}]{title($"{winner.TeamName} Won!")}[/color]";
		endScoreLabel.Show();

		GD.Print(jsonDataHistory.Last());
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

	private Line2D DrawCircle(Vector2I tilemapPosition, int radius, Color color)
	{
		var line = new Line2D
		{
			Name = "Circle",
			Position = tilemapPosition,
			Width = 3,
			Antialiased = true,
			DefaultColor = Godot.Color.FromHtml(color.ColorToHtml()),
			ZIndex = 4,
		};

		for (int i = 0; i < 361; i++)
		{
			var angle = Mathf.DegToRad((float)(1.0 * i));
			line.AddPoint(CalcPointOnCircle(angle, CalcRadiusFromTile(tilemapPosition, radius)));
		}
		return line;
	}

	private float CalcRadiusFromTile(Vector2I tilemapPosition, int radius)
	{
		var godotPosition = tileMapLayer!.MapToLocal(tilemapPosition);
		var tilemapPositionWithOffset =
			tileMapLayer.MapToLocal(new Vector2I(tilemapPosition.X + radius, tilemapPosition.Y));
		return tilemapPositionWithOffset.X - godotPosition.X;
	}

	private static Vector2 CalcPointOnCircle(float angle, float radius)
	{
		return new Vector2(Mathf.Sin(angle) * radius, Mathf.Cos(angle) * radius);
	}

	private void UpdateCurrentTick(int tick)
	{
		currentTick = tick;
		GetNode<RichTextLabel>("%Tick").Text = tick.ToString() + "\n\n";
	}

	private void UpdateScores(List<Score> scores)
	{
		var scoreNode = GetNode<RichTextLabel>("%Score");
		var newText = string.Join("\n", scores.Select(s =>
					$"[color={s.TeamColor.ColorToHtml()}]{s.TeamName}: {s.TeamScore}[/color]"));
		scoreNode.Text = newText;
	}

	private void DrawGame(AgentJsonData parsed)
	{
		DrawAgents(parsed.Agents);
		DrawItems(parsed.Items);
		DrawBarrels(parsed.Barrels);
	}


	private readonly Dictionary<string, Agent> existingAgents = [];
	private readonly Dictionary<string, Tween> agentTweens = [];

	private void DrawAgents(List<Agent> jsonAgents)
	{
		if (existingAgents.Count == 0) InitializeAgents(jsonAgents);

		UpdateAgents(jsonAgents);
	}

	private void InitializeAgents(List<Agent> jsonAgents)
	{
		var names = Names.SelectRandomNames(6);
		for (int i = 0; i < jsonAgents.Count; i++)
		{
			var jsonAgent = jsonAgents[i];
			var name = names[i];

			var agentInstance = agentScene.Instantiate<Agent>();
			agentInstance.Name = name;
			agentInstance.Color = jsonAgent.Color;
			agentInstance.TeamColor = jsonAgent.Color;
			var agentLabel = agentInstance.GetNode<RichTextLabel>("%Label");
			agentLabel.Text = $"[outline_size=12][color={agentInstance.Color.ColorToHtml()}]{name}[/color][/outline_size]";
			agentInstance.Position = tileMapLayer!.MapToLocal(new(jsonAgent.X, map!.Size().Y - 1 - jsonAgent.Y));
			if (jsonAgent.Alive)
			{
				agentInstance.GetNode<Sprite2D>("Sprite2D").Texture = jsonAgent.GetSprite();
			}
			else
			{
				var agentSprite = agentInstance.GetNode<Sprite2D>("Sprite2D");
				agentSprite.Texture = tileSetSpritesheet!.Texture;
				agentSprite.RegionEnabled = true;
				agentSprite.RegionRect = tileSetSpritesheet.GetTileTextureRegion(new(25, 19));
				agentSprite.RotationDegrees += 270;
				jsonAgent.Color = Color.Grey;
			}
			agentInstance.AddChild(DrawCircle(new Vector2I(jsonAgent.X, jsonAgent.Y), jsonAgent.VisualRange, jsonAgent.Color));

			agentInstance.ZIndex = 1;
			agentInstance.ZAsRelative = true;
			tileMapLayer.AddChild(agentInstance);

			existingAgents[jsonAgent.Id] = agentInstance;
		}
	}

	private void UpdateAgents(List<Agent> jsonAgents)
	{
		foreach (var jsonAgent in jsonAgents)
		{
			var agent = existingAgents[jsonAgent.Id];

			if (agent.Alive && !jsonAgent.Alive)
			{
				if (jsonAgent.TaggerId == "00000000-0000-0000-0000-000000000000")
					GetNode<KillFeed>("%KillFeed").AddKill(agent);
				else
				{
					var killer = existingAgents[jsonAgent.TaggerId];
					GetNode<KillFeed>("%KillFeed").AddKill(killer, agent);
				}
			}

			var targetPosition = tileMapLayer!.MapToLocal(new Vector2I(jsonAgent.X, map!.Size().Y - 1 - jsonAgent.Y));

			// kill existing tween if it took too long
			if (agentTweens.TryGetValue(jsonAgent.Id, out var existingTween) && existingTween.IsValid())
			{
				existingTween.Kill();
			}

			// interpolate between positions
			var tween = CreateTween();
			tween.TweenProperty(agent, "position", targetPosition, AgentMoveDuration)
				.SetTrans(Tween.TransitionType.Linear)
				.SetEase(Tween.EaseType.Out);
			agentTweens[jsonAgent.Id] = tween;

			if (jsonAgent.Alive)
			{
				var agentSprite = agent.GetNode<Sprite2D>("Sprite2D");
				agentSprite.Texture = jsonAgent.GetSprite();
				agentSprite.RegionEnabled = false;

			}
			else
			{
				var agentSprite = agent.GetNode<Sprite2D>("Sprite2D");
				agentSprite.Texture = tileSetSpritesheet!.Texture;
				agentSprite.RegionEnabled = true;
				agentSprite.RegionRect = tileSetSpritesheet.GetTileTextureRegion(new(25, 19));
			}

			agent.Alive = jsonAgent.Alive;
			agent.Color = jsonAgent.Color;
			agent.GetNode<Line2D>("Circle").Free();
			agent.AddChild(DrawCircle(new Vector2I(jsonAgent.X, jsonAgent.Y), jsonAgent.VisualRange, jsonAgent.Color));
		}
	}

	private readonly Dictionary<string, Item> existingItems = [];

	private void DrawItems(List<Item> items)
	{
		foreach (var item in items)
		{
			if (existingItems.ContainsKey(item.Id))
			{
				var oldItem = tileMapLayer!.GetNode<Sprite2D>(item.Id);
				oldItem.Position =
					tileMapLayer.MapToLocal(new(item.X, map!.Size().Y - 1 - item.Y));
				continue;
			}

			var flag = new Sprite2D
			{
				Name = item.Id,
				UniqueNameInOwner = true,
				Position = tileMapLayer!.MapToLocal(new(item.X, map!.Size().Y - 1 - item.Y)),
				Texture = tileSetSpritesheet!.Texture,
				RegionEnabled = true,
				RegionRect = item.Color switch
				{
					Color.Red => tileSetSpritesheet.GetTileTextureRegion(new(26, 2)),
					Color.Green => throw new NotImplementedException(),
					Color.Blue => tileSetSpritesheet.GetTileTextureRegion(new(26, 7)),
					Color.Yellow => tileSetSpritesheet.GetTileTextureRegion(new(26, 4)),
					Color.Grey => tileSetSpritesheet.GetTileTextureRegion(new(26, 1)),
					_ => throw new UnreachableException(),
				},
				ZAsRelative = true,
				ZIndex = 2,
			};
			flag.AddToGroup("items");
			existingItems[item.Id] = item;

			tileMapLayer.AddChild(flag);

		}

	}

	private readonly Dictionary<string, Barrel> existingBarrels = [];

	private void DrawBarrels(List<Barrel> barrels)
	{
		var bamBarrelSpriteIndex = new Vector2I(25, 17);
		var explodedBarrelSpriteIndex = new Vector2I(26, 16);
		foreach (var newBarrel in barrels)
		{

			if (!existingBarrels.TryGetValue(newBarrel.Id, out var oldBarrel))
			{
				oldBarrel = newBarrel;
				// TODO: figure out a way to do this in the barrel class
				oldBarrel.Position = tileMapLayer!.MapToLocal(new(newBarrel.X, map!.Size().Y - 1 - newBarrel.Y));
				oldBarrel.RegionEnabled = true;
				oldBarrel.RegionRect = tileSetSpritesheet!.GetTileTextureRegion(new(25, 16));
				oldBarrel.Texture = tileSetSpritesheet!.Texture;
				oldBarrel.ZAsRelative = true;
				oldBarrel.ZIndex = 2;
				existingBarrels[newBarrel.Id] = oldBarrel;
				tileMapLayer!.AddChild(oldBarrel);
			}

			if (oldBarrel.HasExploded)
			{
				oldBarrel.RegionRect = tileSetSpritesheet!.GetTileTextureRegion(explodedBarrelSpriteIndex);
				continue;
			}

			if (!newBarrel.HasExploded) continue;
			oldBarrel.HasExploded = true;
			// show bam sprite
			oldBarrel.RegionRect = tileSetSpritesheet!.GetTileTextureRegion(bamBarrelSpriteIndex);

			var blastRadius = DrawCircle(
				new Vector2I(newBarrel.X, newBarrel.Y),
				BarrelRadius,
				Color.Red
			);
			blastRadius.Width = 5;
			tileMapLayer!.AddChild(blastRadius);

			var tween = CreateTween();
			// modulate:a is a [NodePath](https://docs.godotengine.org/en/stable/classes/class_nodepath.html#class-nodepath)
			// and points to the alpha of the circle
			tween.TweenProperty(blastRadius, "modulate:a", 0f, 255f);
			tween.TweenCallback(Callable.From(() =>
			{
				oldBarrel.RegionRect = tileSetSpritesheet.GetTileTextureRegion(explodedBarrelSpriteIndex);
				blastRadius.QueueFree();
			}));
		}
	}
}
