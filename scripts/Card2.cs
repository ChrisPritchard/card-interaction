using Godot;

public partial class Card2 : MeshInstance3D
{
    private StandardMaterial3D material, counter_material;
    private Area3D area, counter;
    private Label label;

    public int ZIndex { get => material?.RenderPriority ?? 0; set => SetRenderPriority(value); }

    public bool ReceiveShadows { get => !material?.DisableReceiveShadows ?? false; set => material.DisableReceiveShadows = counter_material.DisableReceiveShadows = !value; }

    public override void _Ready()
    {
        area = GetNode<Area3D>("Area3D");
        counter = GetNode<Area3D>("CounterArea3D");

        var existing = GetActiveMaterial(0);
        material = (StandardMaterial3D)existing.Duplicate();
        SetSurfaceOverrideMaterial(0, material);

        var counter_mesh = GetNode<MeshInstance3D>("Counter");

        var existing_counter = counter_mesh.GetActiveMaterial(0);
        counter_material = (StandardMaterial3D)existing_counter.Duplicate();
        counter_mesh.SetSurfaceOverrideMaterial(0, counter_material);

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
        counter_material.RenderPriority = value;

        label.Text = value.ToString();
    }

    public void SetCollisionLayer(int layer)
    {
        area.CollisionLayer = (uint)(1 << (layer - 1));
        counter.CollisionLayer = (uint)(1 << (layer - 1));
    }

}
