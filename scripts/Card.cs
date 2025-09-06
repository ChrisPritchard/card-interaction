
using System;
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
                Material.SetShaderParameter("depth_setting", value);
        }
    }

    private ShaderMaterial Material { get => GetSurfaceOverrideMaterial(0) as ShaderMaterial; }

    public override void _Ready()
    {
        SetSurfaceOverrideMaterial(0, (Material)GetActiveMaterial(0).Duplicate());
        Depth = depth;

        CollisionArea.MouseEntered += OnMouseEntered;
        CollisionArea.MouseExited += OnMouseExited;
    }

    private void OnMouseEntered()
    {
        Material.SetShaderParameter("fade_amount", 1);
    }

    private void OnMouseExited()
    {
        Material.SetShaderParameter("fade_amount", 0);
    }

    internal void SetCollisionLayer(int layer) => CollisionArea.CollisionLayer = (uint)(1 << (layer - 1));

    internal Vector2 GetSize() => new(GetAabb().Size.X, GetAabb().Size.Z);
}