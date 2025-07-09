using Godot;
using mmvp.src.agent;
using Color = mmvp.src.agent.Color;

namespace mmvp;

public partial class KillFeed : RichTextLabel
{
        private RichTextLabel? killFeedLabel;
        private readonly List<string> killEntries = [];

        private const int OUTLINE_SIZE = 12;
        private const float DARKENED_AMOUNT = 0.8f;

        public override void _Ready()
        {
                killFeedLabel = GetNode<RichTextLabel>("%KillFeed");
                killFeedLabel.BbcodeEnabled = true;
        }

        public void AddKill(string killerName, string victimName, Color killerColor, Color victimColor)
        {
                var killText =
                        $"[outline_color={Godot.Color.FromHtml(killerColor.ColorToHtml()).Darkened(DARKENED_AMOUNT).ToHtml(false)}][color={killerColor.ColorToHtml()}]{killerName}[/color][/outline_color] eliminated [outline_color={Godot.Color.FromHtml(victimColor.ColorToHtml()).Darkened(DARKENED_AMOUNT).ToHtml(false)}][color={victimColor.ColorToHtml()}]{victimName}[/color][/outline_color]";

                InternalAddKill(killText);

        }

        private void UpdateDisplay()
        {
                killFeedLabel!.Text = string.Join("\n", killEntries);
        }

        private void InternalAddKill(string killText)
        {
                killEntries.Add($"[outline_size={OUTLINE_SIZE}]{killText}[/outline_size]");

                UpdateDisplay();
        }

        internal void AddKill(StringName name, Color color)
        {
                // TODO: outline colour isn't working, since darkened takes a float and not an int
                InternalAddKill($"[outline_color={Godot.Color.FromHtml(color.ColorToHtml()).Darkened(DARKENED_AMOUNT).ToHtml(false)}][color={color.ColorToHtml()}]{name}[/color][/outline_color] died");
        }
}
