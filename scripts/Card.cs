
using Godot;

public partial class Card : Sprite3D
{
    [Export] public Area3D CollisionArea;

    internal void SetCollisionLayer(int layer) => CollisionArea.CollisionLayer = (uint)(0 << (layer - 1));

    internal Vector2 GetSize() => Texture.GetSize() * PixelSize;
}