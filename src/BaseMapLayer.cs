using Godot;
using System;

namespace BaseMapLayer;

public partial class BaseMapLayer : TileMapLayer
{
    public override void _Ready()
    {
        var mainSubViewportContainer = GetNode<SubViewportContainer>("%MainSubViewportContainer");
        GetNode<SubViewport>("%MainSubViewportContainer/MainSubViewport").SizeChanged += () =>
        {
            var usedRect = GetUsedRect();
            var tileMapSize = usedRect.Size * TileSet.TileSize;

            var scaleX = mainSubViewportContainer.Size.X / tileMapSize.X;
            var scaleY = mainSubViewportContainer.Size.Y / tileMapSize.Y;
            GD.Print(scaleX, scaleY);
            var minScale = Math.Min(scaleX, scaleY);
            GD.Print("minScale: ", minScale);

            Scale = new Vector2(minScale, minScale);
        };
    }
}
