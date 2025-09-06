
using System.Threading.Tasks;
using Godot;

public partial class Card : MeshInstance3D
{
    [Export] public Area3D CollisionArea;


    internal void SetCollisionLayer(int layer) => CollisionArea.CollisionLayer = (uint)(0 << (layer - 1));

    internal Vector2 GetSize() => new(GetAabb().Size.X, GetAabb().Size.Z);

    public override void _Ready()
    {
        SetSurfaceOverrideMaterial(0, (Material)GetActiveMaterial(0).Duplicate());
    }

    internal void SetDepth(float value)
    {
        if (!IsNodeReady())
            CallDeferred(nameof(SetDepth), value);
        else
            (GetSurfaceOverrideMaterial(0) as ShaderMaterial).SetShaderParameter("depth_setting", value);
    }
}