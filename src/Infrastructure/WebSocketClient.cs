namespace MarsGridVisualizer;

using Godot;
using MarsGridVisualizer.Infrastructure;

public partial class WebSocketClient
{
	private readonly WebSocketPeer socket = new();
	private const float ReconnectDelaySeconds = 2.0f;

	public event Action? OnConnected;
	public event Action<AgentJsonData>? OnMessage;
	public event Action? OnDisconnected;

	private readonly Adapter adapter = new Adapter();
	private string? address;
	private double timeSinceLastAttempt;
	private bool connected;

	public void Connect(string address)
	{
		this.address = address;
		TryConnect();
	}

	private void TryConnect()
	{
		if (address is null) return;

		var error = socket.ConnectToUrl(address);
		if (error != Error.Ok)
		{
			GD.Print($"Connection attempt failed (Error: {error}). Will retry...");
		}
		timeSinceLastAttempt = 0;
	}

	public void Next(double delta)
	{
		socket.Poll();

		switch (socket.GetReadyState())
		{
			case WebSocketPeer.State.Connecting:
				break;
			case WebSocketPeer.State.Open:
				if (!connected)
				{
					GD.Print("Connected to Simulation.");
					connected = true;
					OnConnected?.Invoke();
				}
				while (socket.GetAvailablePacketCount() > 0)
				{
					var message = socket.GetPacket().GetStringFromUtf8();
					if (string.IsNullOrWhiteSpace(message)) continue;

					var model = adapter.ModelFrom(message);
					OnMessage?.Invoke(model);
				}
				break;
			case WebSocketPeer.State.Closing:
				break;
			case WebSocketPeer.State.Closed:
				var closeCode = socket.GetCloseCode();
				var closeReason = socket.GetCloseReason();

				if (connected)
				{
					GD.Print($"Connection lost (code: {closeCode}, reason: {closeReason}). Reconnecting...");
					connected = false;
					OnDisconnected?.Invoke();
				}

				timeSinceLastAttempt += delta;
				if (timeSinceLastAttempt >= ReconnectDelaySeconds)
				{
					GD.Print("Attempting to reconnect...");
					TryConnect();
				}
				break;
		}
	}

	public void Send(string message) => socket.SendText(message);
	public void Close() => socket.Close();

}

