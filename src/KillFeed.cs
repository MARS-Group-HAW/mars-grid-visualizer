using Godot;
using mmvp.src.agent;
using Color = mmvp.src.agent.Color;

namespace mmvp;

public partial class KillFeed : RichTextLabel
{
        private RichTextLabel? killFeedLabel;
        private List<string> killEntries = [];

        public override void _Ready()
        {
                killFeedLabel = GetNode<RichTextLabel>("%KillFeed");
                killFeedLabel.BbcodeEnabled = true;
        }

        public void AddKill(string killerName, string victimName, Color killerColor, Color victimColor)
        {
                var killText =
                        $"[color={killerColor.ColorToHtml()}]{killerName}[/color] eliminated [color={victimColor.ColorToHtml()}]{victimName}[/color]";

                InternalAddKill(killText);

        }

        private void UpdateDisplay()
        {
                killFeedLabel!.Text = string.Join("\n", killEntries);
        }

        private void InternalAddKill(string killText)
        {
                killEntries.Add(killText);

                if (killEntries.Count > 5)
                        killEntries.RemoveAt(0);

                UpdateDisplay();
        }

        internal void AddKill(StringName name, Color color)
        {
                InternalAddKill($"[color={color.ColorToHtml()}]{name}[/color] died");
        }
}
