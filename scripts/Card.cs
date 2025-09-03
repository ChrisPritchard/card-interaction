using Godot;

public partial class Card : MeshInstance3D
{
    [Export] public StaticBody3D Body;

    public void SetCollisionLayer(uint layer) => Body.CollisionLayer = (uint)(1 << (int)(layer - 1));
}
