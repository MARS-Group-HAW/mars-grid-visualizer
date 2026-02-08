using Godot;
using MarsGridVisualizer.Domain;

namespace MarsGridVisualizer.Presentation;

public partial class Godot2DRenderer : Node, IRenderer
{
	private readonly Dictionary<long, RenderNode> cache = [];
	public BaseMapLayer? TileMapLayer { get; internal set; }

	private partial class RenderNode(Godot.Color colour, string? SpritePath = null) : Sprite2D
	{
		const int radius = 30;

		public override void _Ready()
		{
			if (SpritePath is not null)
				Texture = GD.Load<Texture2D>(SpritePath);
		}

		public override void _Draw()
		{
			DrawCircle(
				new Vector2(0, 0),
				radius,
				colour,
				antialiased: true
			);
		}
	}

	public void Render(State state)
	{
		if (TileMapLayer is null)
			GD.PrintErr($"warn: TileMapLayer is still null!");

		foreach (var agentType in state.AgentTypes.Values)
		{
			var colour = Colours.GetRandom().ToGodotColor();
			foreach (var instance in agentType)
			{
				if (cache.TryGetValue(instance.Id, out var cached))
				{
					cached.Position = TileMapLayer!.MapToLocal(
						new Vector2I(instance.X, instance.Y)
					);
				}
				else
				{
					var node = new RenderNode(colour)
					{
						Position = TileMapLayer!.MapToLocal(new Vector2I(instance.X, instance.Y)),
					};
					cache.Add(instance.Id, node);
					TileMapLayer.AddChild(node);
				}
			}
		}
	}
}
