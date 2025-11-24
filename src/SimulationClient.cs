using System.Text.Json.Serialization;
using mmvp.src.agent;

namespace mmvp.src;

public record AgentJsonData
{
    [JsonPropertyName("mapPath")]
    public string? MapPath { get; set; } = null;

    [JsonPropertyName("gameMode")]
    public GameMode? GameMode { get; set; } = null;

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

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Color
{
    Red,
    Green,
    Blue,
    Yellow,
    Grey,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GameMode
{
    CaptureTheFlag,
    TeamDeathmatch,
}

public record Item(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("x")] int X,
    [property: JsonPropertyName("y")] int Y,
    [property: JsonPropertyName("color")] Color Color,
    [property: JsonPropertyName("type")] ItemType Type,
    [property: JsonPropertyName("pickedUp")] bool PickedUp,
    [property: JsonPropertyName("ownerID")] string OwnerId
);

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ItemType
{
    Flag,
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
