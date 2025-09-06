
using Godot;

public partial class Card : MeshInstance3D
{
    [Export] public float BorderTransitionSpeed = 0.5f;

    public sbyte RenderOrder
    {
        get
        {
            if (shaderMaterial == null)
                return 0;
            return (sbyte)shaderMaterial.RenderPriority;
        }
        set
        {
            if (shaderMaterial != null)
                shaderMaterial.RenderPriority = value;
        }
    }

    private ShaderMaterial shaderMaterial;
    private Area3D collisionArea;

    public override void _Ready()
    {
        shaderMaterial = (ShaderMaterial)GetActiveMaterial(0).Duplicate();
        SetSurfaceOverrideMaterial(0, shaderMaterial);

        collisionArea = GetNode<Area3D>("Area3D");
    }

    public void ShowBorder(Color? colour = null)
    {
        if (colour != null)
            shaderMaterial.SetShaderParameter("line_color", colour.Value);

        CreateTween()
            .TweenMethod(Callable.From<float>(v => shaderMaterial.SetShaderParameter("fade_amount", v)), 0.0f, 1.0f, BorderTransitionSpeed)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Sine);
    }

    public void HideBorder()
    {
        CreateTween()
            .TweenMethod(Callable.From<float>(v => shaderMaterial.SetShaderParameter("fade_amount", v)), 1.0f, 0.0f, BorderTransitionSpeed)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Sine);
    }

    public void SetCollisionLayer(int layer) => collisionArea.CollisionLayer = (uint)(1 << (layer - 1));

    public Vector2 GetSize() => new(GetAabb().Size.X, GetAabb().Size.Z);
}