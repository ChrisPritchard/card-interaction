using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class Table : Sprite2D
{
    [Export] public PackedScene CardScene { get; set; }

    private readonly Vector2 target_card_size = new(100, 150);

    private readonly List<Card> cards = [];

    public override void _Ready()
    {
        var gap = new Vector2(target_card_size.X * 0.05f, target_card_size.Y * 0.05f);
        var size = Texture.GetSize();

        for (var i = 0; i < 5; i++)
            for (var j = 0; j < 5; j++)
            {
                var new_card = CardScene.Instantiate<Card>();
                new_card.Table = this;
                new_card.Scale = new(target_card_size.X / new_card.Texture.GetWidth(), target_card_size.Y / new_card.Texture.GetHeight());
                new_card.Position = new(
                    size.X / 2 + (-2.5f * (target_card_size.X + gap.X)) + i * (target_card_size.X + gap.X),
                    size.Y / 2 + (-2.5f * (target_card_size.Y + gap.Y)) + j * (target_card_size.Y + gap.Y));
                AddChild(new_card);
                cards.Add(new_card);
            }
    }

    internal void BringToFront(Card card)
    {
        var max_z = cards.Select(c => c.ZIndex).Max();
        card.ZIndex = max_z + 1;
    }
}
