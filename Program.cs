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

    private async Task<Map> LoadMap()
    {
        var configPath = Map.GetConfigPath();
        if (configPath is null)
        {
            GetNode<Label>("%MapNotFoundLabel").Show();
            var fileDialog = GetNode<FileDialog>("%ConfigFilePopup");
            fileDialog.Show();
            var result = await ToSignal(fileDialog, FileDialog.SignalName.FileSelected);
            configPath = result[0].AsString();
        }

        GetNode<Label>("%MapNotFoundLabel").Hide();
        return Map.ReadInMap(configPath);
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
        GetTree().CallGroup("Agents", "queue_free");
        DrawAgents(parsed.Agents);
        DrawItems(parsed.Items);
    }

    private void DrawAgents(List<Agent> agents)
    {
        foreach (var agent in agents)
        {
            var agentInstance = (Node2D)agentScene.Instantiate();
            agentInstance.Position = tileMapLayer!.MapToLocal(new(agent.X, map!.Size().Y - 1 - agent.Y));
            if (agent.Alive)
                agentInstance.GetNode<Sprite2D>("Sprite2D").Texture = agent.GetSprite();
            else
            {
                var headStoneSprite = agentInstance.GetNode<Sprite2D>("Sprite2D");
                headStoneSprite.Texture = tileSetSpritesheet!.Texture;
                headStoneSprite.RegionEnabled = true;
                headStoneSprite.RegionRect = tileSetSpritesheet.GetTileTextureRegion(new(25, 19));
                headStoneSprite.RotationDegrees += 270;
                agent.Color = Color.Grey;
            }

            agentInstance.ZIndex = 1;
            agentInstance.ZAsRelative = true;
            tileMapLayer.AddChild(agentInstance);
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
                    Color.Grey => throw new NotImplementedException(),
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
}
