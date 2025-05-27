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
    private TileMapLayer tileMapLayer;
    private bool webSocketConnection;
    private Map map;

    public override void _Ready()
    {
        // TODO: properly find map path
        // TODO: move path into map class
        map = Map.ReadInMap();
        // GD.Print("Map: \n", map);


        tileMapLayer = GetNode<TileMapLayer>("%TopDownShooterBaseMap");
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

    private void ConnectWebSocket()
    {
        if (socket.ConnectToUrl("ws://127.0.0.1:8181") != Godot.Error.Ok)
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

                if (currentTick != parsed.ExpectingTick) currentTick = parsed.ExpectingTick;
                DrawGame(parsed);
                GD.Print("currentTick: ", currentTick);
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

    private void DrawGame(AgentJsonData parsed)
    {
        GetTree().CallGroup("Agents", "queue_free");
        DrawAgents(parsed.Agents);
    }

    private void DrawAgents(List<Agent> agents)
    {
        foreach (var agent in agents)
        {
            var agentInstance = (Node2D)agentScene.Instantiate();
            agentInstance.Position = tileMapLayer.MapToLocal(new(agent.X, map.Size().Y - 1 - agent.Y));
            if (agent.Alive)
                agentInstance.GetNode<Sprite2D>("Sprite2D").Texture =
                    agent.Color switch
                    {
                        Color.Red => GD.Load<Texture2D>(
                            "res://assets/kenney_top-down-shooter/PNG/Robot 1/robot1_machine.png"),
                        Color.Green => GD.Load<Texture2D>(
                            "res://assets/kenney_top-down-shooter/PNG/Woman Green/womanGreen_machine.png"),
                        Color.Blue => GD.Load<Texture2D>(
                            "res://assets/kenney_top-down-shooter/PNG/Man Blue/manBlue_machine.png"),
                        Color.Yellow => GD.Load<Texture2D>(
                            "res://assets/kenney_top-down-shooter/PNG/Man Brown/manBrown_machine.png"),
                        Color.Grey => GD.Load<Texture2D>(
                            "res://assets/kenney_top-down-shooter/PNG/Man Blue/manBlue_machine.png"),
                        _ => throw new UnreachableException(),
                    };
            else
            {
                var atlasSource = tileMapLayer.TileSet.GetSource(tileMapLayer.TileSet.GetSourceId(0)) as TileSetAtlasSource;
                var sprite = agentInstance.GetNode<Sprite2D>("Sprite2D");
                sprite.Texture = atlasSource.Texture;
                sprite.RegionEnabled = true;
                sprite.RegionRect = atlasSource.GetTileTextureRegion(new(25, 19));
                sprite.RotationDegrees += 270;
                agent.Color = Color.Grey;
            }

            agentInstance.ZIndex = 1;
            agentInstance.ZAsRelative = true;
            tileMapLayer.AddChild(agentInstance);
        }
    }
}
