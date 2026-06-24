namespace MarsGridVisualizer.Ui;

using Godot;

public enum ConnectionState
{
	Connecting,
	Connected,
	Disconnected,
}

public partial class ConnectionBadge : PanelContainer
{
	private StyleBoxFlat dotStyle = null!;
	private Label statusLabel = null!;

	private static readonly Color connectingColor = Colours.Orange.ToGodotColor();
	private static readonly Color connectedColor = Colours.Green.ToGodotColor();
	private static readonly Color disconnectedColor = Colours.Red.ToGodotColor();

	public override void _Ready()
	{
		dotStyle = (StyleBoxFlat)GetNode<Panel>("HBox/Dot").GetThemeStylebox("panel");
		statusLabel = GetNode<Label>("HBox/StatusLabel");
		SetState(ConnectionState.Connecting);
	}

	public void SetState(ConnectionState state)
	{
		(dotStyle.BgColor, statusLabel.Text) = state switch
		{
			ConnectionState.Connecting => (connectingColor, "Connecting…"),
			ConnectionState.Connected => (connectedColor, "Connected"),
			ConnectionState.Disconnected => (disconnectedColor, "Disconnected"),
			_ => (disconnectedColor, "Unknown"),
		};
	}
}
