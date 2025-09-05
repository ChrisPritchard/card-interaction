using Godot;

public partial class MainScene : Node3D
{
    [Export] public PackedScene CardScene { get; set; }
    [Export] public Camera3D Camera;

    private readonly Vector3 target_card_size = new(1, 0.1f, 1.5f);

    public override void _Ready()
    {
        var gap = 0.05f * target_card_size;

        for (var i = 0; i < 5; i++)
            for (var j = 0; j < 5; j++)
            {
                var new_card = CardScene.Instantiate<Card>();
                AddChild(new_card);
                var native_size = new_card.GetSize();
                new_card.Scale = new(target_card_size.X / native_size.X, target_card_size.Y, target_card_size.Z / native_size.Y);
                new_card.GlobalPosition = new Vector3(i * (target_card_size.X + gap.X), 1f, j * (target_card_size.Z + gap.Z));
            }
    }

}
