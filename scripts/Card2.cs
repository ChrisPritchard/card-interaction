using Godot;

public partial class Card2 : MeshInstance3D
{
    private BaseMaterial3D material;
    private Area3D area;
    private Label label;

    public int ZIndex { get => material?.RenderPriority ?? 0; set => SetRenderPriority(value); }

    public override void _Ready()
    {
        area = GetNode<Area3D>("Area3D");
        var material = GetActiveMaterial(0);
        this.material = (BaseMaterial3D)material.Duplicate();
        SetSurfaceOverrideMaterial(0, this.material);
        label = GetNode<Label>("%ZIndex");
    }

    private void SetRenderPriority(int value)
    {
        if (material == null)
        {
            CallDeferred(nameof(SetRenderPriority), value);
            return;
        }

        material.RenderPriority = value;
        // SortingOffset = value * 2;

        label.Text = value.ToString();
    }

    public void SetCollisionLayer(int layer) => area.CollisionLayer = (uint)(1 << (layer - 1));

}
