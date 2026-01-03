namespace MarsGridVisualizer;

using Godot;

public partial class WebSocketClient
{
    private readonly WebSocketPeer socket = new();

    public event Action? OnConnected;
    public event Action<string>? OnMessage;
    public event Action<int, string>? OnDisconnected;

    public void Connect(string address)
    {
        if (socket.ConnectToUrl(address) != Error.Ok)
        {
            throw new Exception("Could not connect to WebSocket Server. Is the Simulation running?");
        }
    }

    public void Next()
    {
        socket.Poll();

        switch (socket.GetReadyState())
        {
            case WebSocketPeer.State.Connecting:
                GD.Print("Connecting to Simulation..");
                break;
            case WebSocketPeer.State.Open:
                while (socket.GetAvailablePacketCount() > 0)
                {
                    var message = socket.GetPacket().GetStringFromUtf8();
                    if (string.IsNullOrWhiteSpace(message)) continue;

                    OnMessage?.Invoke(message);
                }
                break;
            case WebSocketPeer.State.Closed:
                // TODO: Figure out what I want to happen when the socket closed
                GD.PrintErr($"WebSocket closed with code: {socket.GetCloseCode()} and reason: {socket.GetCloseReason()}");
                break;

            default:
                // TODO: missing case is Closing
                // Summary:
                //     The connection is in the process of closing. This means a close request has been
                //     sent to the remote peer but confirmation has not been received.
                break;
        }
    }

    public void Send(string message) => socket.SendText(message);
    public void Close() => socket.Close();

}

