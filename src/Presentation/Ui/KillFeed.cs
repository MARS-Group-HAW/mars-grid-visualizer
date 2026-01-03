using Godot;
using MarsGridVisualizer.Agents;

namespace MarsGridVisualizer.Ui;

public partial class KillFeed : RichTextLabel
{
	private RichTextLabel? killFeedLabel;
	private readonly List<string> killEntries = [];
	private const int OutlineSize = 12;
	private const float DarkenedAmount = 0.8f;

	public override void _Ready()
	{
		killFeedLabel = GetNode<RichTextLabel>("%KillFeed");
		killFeedLabel.BbcodeEnabled = true;
	}

	public override void _Input(InputEvent @event)
	{

	}

	public void AddKill(Agent killer, Agent victim)
	{
		var killText = $"{AddOutlineAndColor(killer)} eliminated {AddOutlineAndColor(victim)}";
		InternalAddKill(killText);
	}

	internal void AddKill(Agent agent)
	{
		InternalAddKill($"{AddOutlineAndColor(agent.Name, agent.TeamColor)} died");
	}

	internal void AddKill(string text, Color color)
	{
		InternalAddKill($"{AddOutlineAndColor(text, color)} died");
	}

	private void InternalAddKill(string killText)
	{
		killEntries.Add($"{killText}");
		killFeedLabel!.Text = string.Join("\n", killEntries);
	}

	private static string AddOutlineAndColor(Agent agent)
	{
		return AddOutlineAndColor(agent.Name, agent.TeamColor);
	}

	private static string AddOutlineAndColor(string text, Color color)
	{
		var htmlColor = color.ToGodotColor().Darkened(DarkenedAmount).ToHtml(false);
		return $"[outline_size={OutlineSize}][outline_color={htmlColor}][color={color.ColorToHtml()}]{text}[/color][/outline_color][/outline_size]";
	}
}
