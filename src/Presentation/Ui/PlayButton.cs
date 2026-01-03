namespace MarsGridVisualizer.Ui;

using Godot;

public partial class PlayButton : Button
{
    [Signal]
    public delegate void PausedChangedEventHandler(bool isPaused);

    private readonly Texture2D playIcon = GD.Load<Texture2D>("res://assets/icons/Play.svg");
    private readonly Texture2D pauseIcon = GD.Load<Texture2D>("res://assets/icons/Pause.svg");

    public override void _Ready()
    {
        Toggled += OnToggled;
        Icon = pauseIcon;
    }

    private void OnToggled(bool buttonPressed)
    {
        Icon = buttonPressed ? playIcon : pauseIcon;
        EmitSignal(SignalName.PausedChanged, buttonPressed);
    }
}
