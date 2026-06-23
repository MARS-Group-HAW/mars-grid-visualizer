using System.Text.Json.Serialization;
using MarsGridVisualizer.Agents;
using Godot;

namespace MarsGridVisualizer.LaserTag;

public record AgentJsonData
{
	[JsonPropertyName("mapPath")] public required string MapPath { get; set; }
	[JsonPropertyName("gameMode")] public GameMode? GameMode { get; set; } = null;
	[JsonPropertyName("expectingTick")] public required int ExpectingTick { get; set; }
	[JsonPropertyName("agents")] public List<Agent> Agents { get; set; } = [];
	[JsonPropertyName("items")] public List<Item> Items { get; set; } = [];
	[JsonPropertyName("explosiveBarrels")] public List<Barrel> Barrels { get; set; } = [];
	[JsonPropertyName("scores")] public List<Score> Scores { get; set; } = [];
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Color { Red, Green, Blue, Yellow, Grey }

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GameMode { CaptureTheFlag, TeamDeathmatch }

public record Item(
	[property: JsonPropertyName("id")] string Id,
	[property: JsonPropertyName("x")] int X,
	[property: JsonPropertyName("y")] int Y,
	[property: JsonPropertyName("color")] Color Color,
	[property: JsonPropertyName("type")] ItemType Type,
	[property: JsonPropertyName("pickedUp")] bool PickedUp,
	[property: JsonPropertyName("ownerID")] string OwnerId
);

public partial class Barrel(string id, int x, int y, bool hasExploded) : Sprite2D
{
	[JsonPropertyName("id")] public string Id { get; set; } = id;
	[JsonPropertyName("x")] public int X { get; set; } = x;
	[JsonPropertyName("y")] public int Y { get; set; } = y;
	[JsonPropertyName("hasExploded")] public bool HasExploded { get; set; } = hasExploded;
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ItemType { Flag }

public class Score
{
	[JsonPropertyName("teamName")] public string TeamName { get; set; } = "";
	[JsonPropertyName("teamColor")] public Color TeamColor { get; set; } = Color.Grey;
	[JsonPropertyName("score")] public int TeamScore { get; set; } = 0;

	public override string ToString()
	{
		return $"Score {{ {TeamName}, {TeamScore} }}";
	}
}
