using System;
using System.Collections.Generic;
using Godot;
using System.Text.Json.Serialization;


namespace mmvp.src.agent;

public enum Stance
{
    Standing,
    Crouching,
    Creeping,
}

public class Agent(int x, int y)
{
    [JsonPropertyName("x")]
    public int X { get; set; } = x;
    [JsonPropertyName("y")]
    public int Y { get; set; } = y;
    [JsonPropertyName("alive")]
    public bool Alive { get; set; } = true;
    [JsonPropertyName("color")]
    public string Color { get; set; } = "Dead";
    [JsonPropertyName("team")]
    public string Team { get; set; } = null;
    [JsonPropertyName("visualRange")]
    public int VisualRange { get; set; } = 10;
    [JsonPropertyName("gotShot")]
    public bool GotShot { get; set; } = false;
    [JsonPropertyName("stance")]
    public Stance Stance { get; set; } = Stance.Standing;
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
