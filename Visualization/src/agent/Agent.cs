using System;
using System.Collections.Generic;
using Godot;
using System.Text.Json.Serialization;

namespace mmvp.src.agent;


public partial class Agent : Node2D
{
    public enum Stance
    {
        Standing,
        Crouching,
        Creeping,
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Color
    {
        Red,
        Green,
        Blue,
        Yellow,
        Grey,
    }

    [JsonPropertyName("x")]
    public int X { get; set; }
    [JsonPropertyName("y")]
    public int Y { get; set; }
    [JsonPropertyName("alive")]
    public bool Alive { get; set; } = true;
    [JsonPropertyName("color")]
    public Color TeamColor { get; set; } = Color.Grey;
    [JsonPropertyName("team")]
    public string Team { get; set; } = null;
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
    [JsonPropertyName("tick")]
    public int Tick { get; set; }

    [JsonPropertyName("agents")]
    public List<Agent> Agents { get; set; } = [];

    // TODO: add items and barrels

    // FIXME: for some reason scores are empty
    [JsonPropertyName("scores")]
    public List<Score> Scores { get; set; } = [];
}

public class Score
{
    [JsonPropertyName("teamName")]
    public string TeamName;
    [JsonPropertyName("teamColor")]
    public string TeamColor;
    [JsonPropertyName("score")]
    public int TeamScore;
}
