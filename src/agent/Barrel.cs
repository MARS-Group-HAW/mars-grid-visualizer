using Godot;
using System.Text.Json.Serialization;

namespace mmvp.src.agent;

public partial class Barrel(
    string id,
    int x,
    int y,
    bool hasExploded) : Sprite2D
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = id;
    [JsonPropertyName("x")]
    public int X { get; set; } = x;
    [JsonPropertyName("y")]
    public int Y { get; set; } = y;
    [JsonPropertyName("hasExploded")]
    public bool HasExploded { get; set; } = hasExploded;
}

