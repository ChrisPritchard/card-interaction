using Godot;

public partial class MainScene : Node3D
{
    [Export] public PackedScene CardScene { get; set; }
    [Export] public MeshInstance3D Table { get; set; }

    public override void _Ready()
    {
        var card_size = CardScene.Instantiate<Card>().GetAabb();
        var cw = card_size.Size.X;
        var gap = cw * 0.05f;

        for (var i = 0; i < 5; i++)
        {
            var new_card = CardScene.Instantiate<Card>();
            new_card.Position = new((-2.5f * cw) + i * cw + i * gap, 0, 0);
            AddChild(new_card);
        }
    }

}
