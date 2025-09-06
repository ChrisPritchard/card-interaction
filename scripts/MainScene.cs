using System.Collections.Generic;
using Godot;

public partial class MainScene : Node3D
{
    [Export] public PackedScene CardScene { get; set; }

    private readonly Vector2 target_card_size = new(1, 1.5f);

    private readonly List<Card> cards = [];

    private const float depth_adjust = 0.01f;

    public override void _Ready()
    {
        var gap = 0.05f * target_card_size;

        for (var i = 0; i < 5; i++)
            for (var j = 0; j < 5; j++)
            {
                var new_card = CardScene.Instantiate<Card>();
                AddChild(new_card);

                var native_size = new_card.GetSize();
                new_card.Scale = new(target_card_size.X / native_size.X, 1, target_card_size.Y / native_size.Y);
                new_card.GlobalPosition = new Vector3(i * (target_card_size.X + gap.X), 0, j * (target_card_size.Y + gap.Y));

                cards.Add(new_card);
                new_card.SetCollisionLayer(1);
                new_card.Depth = cards.Count * depth_adjust;
            }
    }

}
