using System.Diagnostics;
using System.Text.Json.Serialization;
using Godot;

namespace MarsGridVisualizer.Agents;

public static class ColorMethods
{
    public static string ColorToHtml(this Color color)
    {
        return color switch
        {
            Color.Red => "#e86a17",
            Color.Green => "#27ae60",
            Color.Blue => "#2a87bc",
            Color.Yellow => "#ffcc00",
            Color.Grey => "#5f5f5f",
            _ => throw new UnreachableException(),
        };
    }

    public static Godot.Color ToGodotColor(this Color color)
    {
        return Godot.Color.FromHtml(color.ColorToHtml());
    }
}

/// <summary>
/// Used for JSON deserialization.
/// </summary>
public record AgentData;

public partial class Agent : Node2D
{
    public enum Stance
    {
        Standing,
        Crouching,
        Creeping,
    }

    [JsonPropertyName("id")] public string Id { get; set; } = "00000000-0000-0000-0000-000000000000";
    [JsonPropertyName("x")] public int X { get; set; }
    [JsonPropertyName("y")] public int Y { get; set; }
    [JsonPropertyName("alive")] public bool Alive { get; set; } = true;
    [JsonPropertyName("color")] public Color Color { get; set; } = Color.Grey;
    [JsonPropertyName("team")] private string Team { get; set; } = "";
    [JsonPropertyName("visualRange")] public int VisualRange { get; set; } = 10;
    [JsonPropertyName("gotShot")] private bool GotShot { get; set; } = false;
    [JsonPropertyName("stance")] private Stance CurrentStance { get; set; } = Stance.Standing;
    [JsonPropertyName("taggerID")] public string TaggerId { get; set; } = "00000000-0000-0000-0000-000000000000";
    public Color TeamColor { get; internal set; }

    public Agent() { }

    public Agent(int x, int y)
    {
        X = x;
        Y = y;
    }

    public Texture2D GetSprite()
    {
        return Color switch
        {
            Color.Red => GD.Load<Texture2D>("res://assets/agents/red/manRed_machine.png"),
            Color.Green => GD.Load<Texture2D>("res://assets/kenney_top-down-shooter/PNG/Woman Green/womanGreen_machine.png"),
            Color.Blue => GD.Load<Texture2D>("res://assets/kenney_top-down-shooter/PNG/Man Blue/manBlue_machine.png"),
            Color.Yellow => GD.Load<Texture2D>("res://assets/agents/yellow/manYellow_machine.png"),
            Color.Grey => GD.Load<Texture2D>("res://assets/kenney_top-down-shooter/PNG/Man Blue/manBlue_machine.png"),
            _ => throw new UnreachableException(),
        };
    }
}
