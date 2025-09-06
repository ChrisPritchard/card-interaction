
using System;
using Godot;

public partial class Card : MeshInstance3D
{
    [Export] public Area3D CollisionArea;

    [Export] public float BorderTransitionSpeed = 0.8f;

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
    }

    public void ShowBorder(Color? colour = null)
    {
        if (colour != null)
            Material.SetShaderParameter("line_color", colour.Value);
        CreateTween()
            .TweenMethod(Callable.From<float>(v => Material.SetShaderParameter("fade_amount", v)), 0.0f, 1.0f, BorderTransitionSpeed)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Sine);
    }

    public void HideBorder()
    {
        CreateTween()
            .TweenMethod(Callable.From<float>(v => Material.SetShaderParameter("fade_amount", v)), 1.0f, 0.0f, BorderTransitionSpeed)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Sine);
    }

    internal void SetCollisionLayer(int layer) => CollisionArea.CollisionLayer = (uint)(1 << (layer - 1));

    internal Vector2 GetSize() => new(GetAabb().Size.X, GetAabb().Size.Z);
}