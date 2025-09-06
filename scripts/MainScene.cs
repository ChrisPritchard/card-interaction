using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class MainScene : Node3D
{
    private PackedScene cardScene;

    private readonly Vector2 card_size = new(1, 1.5f);

    private readonly List<Card> cards = [];

    private const sbyte card_render_order_start = 0;

    public override void _Ready()
    {
        cardScene = GD.Load<PackedScene>("res://scenes/card.tscn");
        var gap = 0.05f * card_size;

        for (var i = 0; i < 5; i++)
            for (var j = 0; j < 5; j++)
            {
                var new_card = cardScene.Instantiate<Card>();
                AddChild(new_card);

                var native_size = new_card.GetSize();
                new_card.Scale = new(card_size.X / native_size.X, 1, card_size.Y / native_size.Y);
                new_card.GlobalPosition = new Vector3(i * (card_size.X + gap.X), 0, j * (card_size.Y + gap.Y));

                cards.Add(new_card);
                new_card.SetCollisionLayer(1);
            }
    }

    /// <summary>
    /// Render order is an sbyte, -127 to 126 or something. if every card is perfectly compressed in order, so values only diff by 1, 
    /// this makes an upper limit of 256 cards (ignoring the table or anything else that needs render ordering).
    /// This function solves this by realising that if two cards are not overlapping, their render order is irrelevant (as long as its 
    /// in front of the table) - e.g. they could be the same or whatever.
    /// Only the cards that overlap each other need their order to be different, so this function finds those groups and compresses their order individually.
    /// </summary>
    public void ReSortCards()
    {
        var n = cards.Count;
        var parent = new int[n];
        for (int i = 0; i < n; i++)
            parent[i] = i; // every element is in its own group, initially (its parent is itself, so its the root elem)

        Rect2 Rect(Card c) => new(new(c.Position.X, c.Position.Z), card_size);

        // find with path compression (finds the root, and while doing so, ensures each element of the group also points to this root)
        int Find(int x) => parent[x] == x ? x : parent[x] = Find(parent[x]);

        // if two elements (x, y) have different roots, one root is made the parent of the other (groups therefore joined)
        void Union(int x, int y)
        {
            int rootX = Find(x);
            int rootY = Find(y);
            if (rootX != rootY)
                parent[rootY] = rootX;
        }

        // using the above functions, whenever cards overlap other cards, ensure all such cards have the same root
        for (var i = 0; i < n; i++)
        {
            var rectI = Rect(cards[i]);
            for (int j = i + 1; j < n; j++)
            {
                var rectJ = Rect(cards[j]);
                if (rectI.Intersects(rectJ))
                    Union(i, j);
            }
        }

        // group objects by their root parent
        var groupsDict = new Dictionary<int, List<Card>>();
        for (var i = 0; i < n; i++)
        {
            var root = Find(i);
            if (!groupsDict.ContainsKey(root))
                groupsDict[root] = [];
            groupsDict[root].Add(cards[i]);
        }

        // and finally order the cards in each group by their prior order, compressing errant values back down to a reasonable range (render order only supports 255 values)
        foreach (var group in groupsDict.Values)
        {
            if (group.Count == 0)
                group[0].RenderOrder = 0;
            else
                group.OrderBy(c => c.RenderOrder).Select((card, index) => (card, index)).ToList().ForEach(o =>
                    o.card.RenderOrder = (sbyte)(card_render_order_start + (sbyte)o.index));
        }
    }
}
