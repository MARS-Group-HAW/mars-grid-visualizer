using Godot;
using System.Text.Json;

namespace MarsGridVisualizer.Infrastructure;

/** <summary>
 * Takes a message from a simulation and translates it
 * to structures the visualization understands.
 * </summary>
 */
public class Adapter
{
	private readonly JsonSerializerOptions jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
	public AgentJsonData ModelFrom(string message)
	{
		var parsed = JsonSerializer.Deserialize<AgentJsonData>(message, jsonOptions);
		if (parsed == null) { GD.PrintErr("could not serialize json to AgentJsonData"); throw new Exception(); }

		return parsed;
	}
}
