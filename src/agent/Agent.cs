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

    [JsonPropertyName("x")]
    public int X { get; set; }
    [JsonPropertyName("y")]
    public int Y { get; set; }
    [JsonPropertyName("alive")]
    public bool Alive { get; set; } = true;
    [JsonPropertyName("color")]
    public Color Color { get; set; } = Color.Grey;
    [JsonPropertyName("team")]
    public string Team { get; set; } = "";
    [JsonPropertyName("visualRange")]
    public int VisualRange { get; set; } = 10;
    [JsonPropertyName("gotShot")]
    public bool GotShot { get; set; } = false;
    [JsonPropertyName("stance")]
    public Stance CurrentStance { get; set; } = Stance.Standing;

    public Agent() { }
    public Agent(int x, int y)
    {
        X = x;
        Y = y;
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

public record Barrel(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("x")] int X,
        [property: JsonPropertyName("y")] int Y,
        [property: JsonPropertyName("hasExploded")] bool HasExploded);

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
