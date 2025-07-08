using System.Diagnostics;
using System.Text.Json;
using Godot;
using mmvp.src;
using mmvp.src.agent;
using Color = mmvp.src.agent.Color;

namespace mmvp;

public partial class Program : Node2D
{
    private readonly JsonSerializerOptions jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private PackedScene agentScene = GD.Load<PackedScene>("res://src/agent/agent.tscn");

    private WebSocketPeer socket = new();
    private int currentTick = 1;
    private TileMapLayer? tileMapLayer;
    private bool webSocketConnection;
    private TileSetAtlasSource? tileSetSpritesheet;
    private Map? map;
    private LaserTagConfig? config;

    public override async void _Ready()
    {
        map = await LoadMap();

        tileMapLayer = GetNode<TileMapLayer>("%TopDownShooterBaseMap");
        tileSetSpritesheet = (TileSetAtlasSource)tileMapLayer.TileSet.GetSource(tileMapLayer.TileSet.GetSourceId(0));
        map.PopulateTileMap(tileMapLayer);

        ConnectWebSocket();
    }

    public override void _Process(double delta)
    {
        if (webSocketConnection) WebSocketLoop();
    }

    public override void _Input(InputEvent @event)
    {
        if (IsInstanceValid(@event) &&
                @event is InputEventKey key &&
                key.Keycode == Key.Escape)
        {
            GetTree().Quit();
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

    private async Task<Map> LoadMap()
    {
        var configPath = LaserTagConfig.GetConfigPath();
        var mapPath = configPath is not null ? LaserTagConfig.GetMapPath(configPath) : null;
        if (mapPath is null)
        {
            GetNode<Label>("%MapNotFoundLabel").Show();
            var fileDialog = GetNode<FileDialog>("%ConfigFilePopup");
            fileDialog.Show();
            var result = await ToSignal(fileDialog, FileDialog.SignalName.FileSelected);
            configPath = result[0].AsString();
            mapPath = LaserTagConfig.GetMapPath(configPath)!;
        }
        config = LaserTagConfig.New(configPath!);

        GetNode<Label>("%MapNotFoundLabel").Hide();
        return Map.ReadInMap(mapPath);
    }

    private void ConnectWebSocket()
    {
        if (socket.ConnectToUrl("ws://127.0.0.1:8181") != Error.Ok)
        {
            GD.Print("Could not connect to WebSocket Server. Is the Simulation running?");
            GetTree().Quit();
        }
        webSocketConnection = true;
        GD.Print("Connected to Simulation.");
    }

    private void WebSocketLoop()
    {
        socket.Poll();

        if (socket.GetReadyState() is WebSocketPeer.State.Open)
        {
            while (socket.GetAvailablePacketCount() > 0)
            {
                var message = socket.GetPacket().GetStringFromUtf8();
                if (string.IsNullOrWhiteSpace(message))
                {
                    continue;
                }
                var parsed = JsonSerializer.Deserialize<AgentJsonData>(message, jsonOptions);
                if (parsed == null) { GD.Print("could not serialize json to AgentJsonData"); continue; }

                if (currentTick != parsed.ExpectingTick) currentTick = parsed.ExpectingTick;
                UpdateScores(parsed.Scores);
                DrawGame(parsed);
                if (currentTick % 100 == 0) GD.Print("currentTick: ", currentTick);
                socket.SendText(currentTick.ToString());
                currentTick += 1;
            }
        }

        if (socket.GetReadyState() is WebSocketPeer.State.Closed)
        {
            GD.Print($"WebSocket closed with code: {socket.GetCloseCode()} and reason: {socket.GetCloseReason()}");
            SetProcess(false);
        }
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
            agentInstance.GetNode<Label>("Label").Text = name;
            agentInstance.Position = tileMapLayer!.MapToLocal(new(jsonAgent.X, map!.Size().Y - 1 - jsonAgent.Y));
            if (jsonAgent.Alive)
            {
                agentInstance.GetNode<Sprite2D>("Sprite2D").Texture = jsonAgent.GetSprite();
                agentInstance.GetNode<Label>("%Label").LabelSettings.FontColor = Godot.Color.FromHtml(jsonAgent.Color.ColorToHtml());
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
                    GetNode<KillFeed>("%KillFeed").AddKill(agent.Name, agent.Color);
                else
                {
                    var killer = existingAgents[jsonAgent.TaggerId];
                    GetNode<KillFeed>("%KillFeed").AddKill(killer.Name, agent.Name, killer.Color, agent.Color);
                }
            }

            agent.Position = tileMapLayer!.MapToLocal(new Vector2I(jsonAgent.X, map!.Size().Y - 1 - jsonAgent.Y));

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
                jsonAgent.Color = Color.Grey;

            }

            agent.Alive = jsonAgent.Alive;
            agent.Color = jsonAgent.Color;
            agent.GetNode<Label>("%Label").LabelSettings.FontColor = Godot.Color.FromHtml(jsonAgent.Color.ColorToHtml());
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
            // after timeout show exploded sprite
            GetTree().CreateTimer(10.0f).Timeout += () =>
                GD.Print("Timer triggered");
            oldBarrel.RegionRect = tileSetSpritesheet.GetTileTextureRegion(explodedBarrelSpriteIndex);
        }
    }
}
