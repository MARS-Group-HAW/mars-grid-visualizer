using Godot;

namespace MarsGridVisualizer;

public partial class BaseMapLayer : TileMapLayer
{
	private SubViewportContainer? mainSubViewportContainer;

	public override void _Ready()
	{
		mainSubViewportContainer = GetNode<SubViewportContainer>("%MainSubViewportContainer");
		GetNode<SubViewport>("%MainSubViewportContainer/MainSubViewport").SizeChanged +=
			UpdateScaleAndPosition;
	}

	public void UpdateScaleAndPosition()
	{
		if (mainSubViewportContainer is null) return;

		var usedRect = GetUsedRect();
		if (usedRect.Size == Vector2I.Zero) return;

		var tileMapSize = usedRect.Size * TileSet.TileSize;

		var scaleX = mainSubViewportContainer.Size.X / tileMapSize.X;
		var scaleY = mainSubViewportContainer.Size.Y / tileMapSize.Y;
		var minScale = Math.Min(scaleX, scaleY);

		Scale = new Vector2(minScale, minScale);

		var tileMapSizeNormalized = new Vector2(
			tileMapSize.X * minScale,
			tileMapSize.Y * minScale
		);
		Position = (mainSubViewportContainer.Size - tileMapSizeNormalized) / 2;
	}
}
