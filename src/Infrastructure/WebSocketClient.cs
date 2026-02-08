namespace MarsGridVisualizer.Infrastructure;

using Godot;

public class WebSocketClient
{
	private readonly WebSocketPeer socket = new();
	private const float ReconnectDelaySeconds = 2.0f;

	public event Action? OnConnected;
	public event Action<string>? OnMessage;
	public event Action<int, string>? OnDisconnected;

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

					OnMessage?.Invoke(message);
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
					OnDisconnected?.Invoke(closeCode, closeReason);
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

