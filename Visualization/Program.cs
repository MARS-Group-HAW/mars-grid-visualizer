using System.Text.Json;
using Godot;
using mmvp.src;
using mmvp.src.agent;

namespace mmvp;

public partial class Program : Node2D
{
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

        tileMapLayer = GetNode<TileMapLayer>("%BaseMap");
        map.PopulateTileMap(tileMapLayer);

        // ConnectWebSocket();

        var parsed = JsonSerializer
            .Deserialize<AgentJsonData>(File.ReadAllText("./msg_test_buffer.json"));

        DrawAgents(parsed.Agents);
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

        if (socket.GetReadyState() == WebSocketPeer.State.Open)
        {
            socket.SendText(currentTick.ToString());
            while (socket.GetAvailablePacketCount() > 0)
            {
                var message = socket.GetPacket().GetStringFromUtf8();
                GD.Print("Message: ", Json.ParseString(message));
            }

        }

        if (socket.GetReadyState() == WebSocketPeer.State.Closed)
        {
            GD.Print($"WebSocket closed with code: {socket.GetCloseCode()} and reason: {socket.GetCloseReason()}");
            SetProcess(false);
        }
    }

    private void DrawAgents(List<Agent> agents)
    {
        foreach (var agent in agents)
        {
            var agentInstance = (Node2D)agentScene.Instantiate();
            agentInstance.Position = tileMapLayer.MapToLocal(new(agent.X, agent.Y));
            agentInstance.ZIndex = 99;
            agentInstance.ZAsRelative = true;

            tileMapLayer.AddChild(agentInstance);
        }
    }
}
