
using Godot;

public partial class Card : MeshInstance3D
{
    [Export] public Area3D CollisionArea;

    private float depth;
    public float Depth
    {
        get => depth;
        set
        {
            depth = value;
            if (IsNodeReady())
                (GetSurfaceOverrideMaterial(0) as ShaderMaterial).SetShaderParameter("depth_setting", value);
        }
    }

    public override void _Ready()
    {
        SetSurfaceOverrideMaterial(0, (Material)GetActiveMaterial(0).Duplicate());
        Depth = depth;
    }

    internal void SetCollisionLayer(int layer) => CollisionArea.CollisionLayer = (uint)(0 << (layer - 1));

    internal Vector2 GetSize() => new(GetAabb().Size.X, GetAabb().Size.Z);
}