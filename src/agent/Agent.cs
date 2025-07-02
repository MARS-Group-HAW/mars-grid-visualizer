using System;
using System.Collections.Generic;
using Godot;
using System.Text.Json.Serialization;
using System.Diagnostics;

namespace mmvp.src.agent;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Color
{
    Red,
    Green,
    Blue,
    Yellow,
    Grey,
}

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
}


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

    public Agent()
    {
    }

    public Agent(int x, int y)
    {
        X = x;
        Y = y;
    }

    public Texture2D GetSprite()
    {
        return Color switch
        {
            Color.Red => GD.Load<Texture2D>(
                "res://assets/agents/red/manRed_machine.png"),
            Color.Green => GD.Load<Texture2D>(
                "res://assets/kenney_top-down-shooter/PNG/Woman Green/womanGreen_machine.png"),
            Color.Blue => GD.Load<Texture2D>(
                "res://assets/kenney_top-down-shooter/PNG/Man Blue/manBlue_machine.png"),
            Color.Yellow => GD.Load<Texture2D>(
                "res://assets/agents/yellow/manYellow_machine.png"),
            Color.Grey => GD.Load<Texture2D>(
                "res://assets/kenney_top-down-shooter/PNG/Man Blue/manBlue_machine.png"),
            _ => throw new UnreachableException(),
        };
    }
}

public class AgentJsonData
{
    [JsonPropertyName("expectingTick")]
    public int ExpectingTick { get; set; } = -1;

    [JsonPropertyName("agents")]
    public List<Agent> Agents { get; set; } = [];

    [JsonPropertyName("items")]
    public List<Item> Items { get; set; } = [];

    [JsonPropertyName("explosiveBarrels")]
    public List<Barrel> Barrels { get; set; } = [];

    [JsonPropertyName("scores")]
    public List<Score> Scores { get; set; } = [];
}

public record Item(
  [property: JsonPropertyName("id")] string Id,
  [property: JsonPropertyName("x")] int X,
  [property: JsonPropertyName("y")] int Y,
  [property: JsonPropertyName("color")] Color Color,
  [property: JsonPropertyName("type")] ItemType Type,
  [property: JsonPropertyName("pickedUp")] bool PickedUp,
  [property: JsonPropertyName("ownerID")] string OwnerId);

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ItemType
{
    Flag
}

public class Score
{
    [JsonPropertyName("teamName")]
    public string TeamName { get; set; } = "";
    [JsonPropertyName("teamColor")]
    public Color TeamColor { get; set; } = Color.Grey;
    [JsonPropertyName("score")]
    public int TeamScore { get; set; } = 0;

    public override string ToString()
    {
        return $"Score {{ {TeamName}, {TeamScore} }}";
    }
}
