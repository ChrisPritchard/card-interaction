using Godot;

public partial class MainScene : Node3D
{
    [Export] public PackedScene CardScene { get; set; }
    [Export] public MeshInstance3D Table { get; set; }

    private readonly Vector3 target_card_size = new(0.5f / 2, 0, 0.75f / 2);

    public override void _Ready()
    {
        var card_size = CardScene.Instantiate<Card>().GetAabb();
        var cw = card_size.Size.X;
        var ch = card_size.Size.Z;
        var gap = cw * 0.05f;

        var scale = new Vector3(target_card_size.X / cw, 1, target_card_size.Z / ch);

        for (var i = 0; i < 5; i++)
            for (var j = 0; j < 5; j++)
            {
                var new_card = CardScene.Instantiate<Card>();
                new_card.Position = new((-2.5f * cw) + i * cw + i * gap, 0.05f, (-2.5f * ch) + j * ch + j * gap);
                new_card.Scale = scale;
                AddChild(new_card);
            }
    }

}
