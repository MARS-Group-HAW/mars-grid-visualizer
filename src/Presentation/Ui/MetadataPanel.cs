using Godot;

namespace MarsGridVisualizer.Presentation.Ui;

public partial class MetadataPanel : VBoxContainer
{
	private readonly Dictionary<string, (VBoxContainer container, Label heading, RichTextLabel value)> rows = [];

	public void SetStat(string id, string heading, string value)
	{
		if (!rows.TryGetValue(id, out var row))
		{
			var container = new VBoxContainer();
			var headingLabel = new Label();
			var valueLabel = new RichTextLabel
			{
				BbcodeEnabled = true,
				FitContent = true,
			};
			container.AddChild(headingLabel);
			container.AddChild(valueLabel);
			AddChild(container);
			row = (container, headingLabel, valueLabel);
			rows[id] = row;
		}
		row.heading.Text = heading;
		row.value.Text = value;
	}
}
