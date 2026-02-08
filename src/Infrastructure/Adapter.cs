using Godot;
using System.Text.Json;
using J = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace MarsGridVisualizer.Infrastructure;

public class JsonModel
{
	[J("currentTick")] public int CurrentTick { get; set; }
	[J("typeName")] public required string TypeName { get; set; }
	[J("t")] public int LayerId { get; set; }
	[J("entities")] public required JsonEntity[] Entities { get; set; }
}

public struct JsonEntity
{
	[J("key")] public long Id { get; set; }
	[J("x")] public int X { get; set; }
	[J("y")] public int Y { get; set; }
	[J("b")] public int B { get; set; }
}

/** <summary>
 * Takes a message from a simulation and translates it
 * to structures the visualization understands.
 * </summary>
 */
public class Adapter
{
	private readonly JsonSerializerOptions jsonOptions = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		PropertyNameCaseInsensitive = true,
	};

	public AgentJsonData ModelFrom(string message)
	{
		var parsed = JsonSerializer.Deserialize<AgentJsonData>(message, jsonOptions);
		if (parsed == null)
		{
			GD.PrintErr("could not serialize json to AgentJsonData");
			throw new Exception();
		}

		return parsed;
	}

	public JsonModel? ModelFromPythonViz(string message)
	{
		var parsed = JsonSerializer.Deserialize<JsonModel>(message, jsonOptions);
		return parsed;
	}
}
