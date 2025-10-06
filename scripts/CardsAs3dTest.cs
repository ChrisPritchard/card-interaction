using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class CardsAs3dTest : Node3D
{
    private PackedScene card_scene;

    private List<Card2> cards = [];

    public override void _Ready()
    {
        camera = GetViewport().GetCamera3D();

        card_scene = GD.Load<PackedScene>("uid://b12p7d773gddj");
        var card_holder = GetNode<Node3D>("%card_holder");

        cards.AddRange(card_holder.GetChildren().OfType<Card2>());

        for (var i = 0; i < 10; i++)
        {
            var new_card = card_scene.Instantiate<Card2>();
            new_card.ZIndex = i;
            cards.Add(new_card);
            card_holder.AddChild(new_card);
        }

        ResortCards();
    }

    private Camera3D camera;

    private Card2 dragged_card;
    private Card2 hover_card;
    private Card2 last_hover_card;
    private Vector3 drag_offset;
    private Vector3 drag_start;

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion move)
        {
            if (dragged_card != null)
                UpdateDragPosition(move.GlobalPosition);
            else
                TestForHover(move.GlobalPosition);
        }
        else if (@event is InputEventMouseButton click && click.ButtonIndex == MouseButton.Left)
        {
            if (click.Pressed && dragged_card == null)
                TryStartDrag(click.GlobalPosition);
            else if (dragged_card != null) // just released
                TryEndDrag(click.GlobalPosition);
        }
    }

    private void TestForHover(Vector2 screenPos)
    {
        var result = Raycast(screenPos);
        if (result == null)
            return;
        var (_, card) = result.Value;
        if (card == null)
        {
            last_hover_card = null; // over empty space - can return and hover over the same card
            //hover_card?.HideBorder();
            hover_card = null;
            return;
        }

        if (card == hover_card || card == last_hover_card)
            return;

        // if (hover_card != null)
        //     hover_card?.HideBorder();

        last_hover_card = card; // can't hover over this card again until another card or empty space is encountered
        hover_card = card;
        //card.ShowBorder();
        GD.Print($"{card.Name} - Z: {card.ZIndex}");
    }

    private void TryStartDrag(Vector2 screenPos)
    {
        var result = Raycast(screenPos);
        if (result == null)
            return;

        var (position, card) = result.Value;
        if (card == null)
            return;

        dragged_card = card;
        drag_start = card.GlobalPosition;
        drag_offset = card.GlobalPosition - position;
        card.ZIndex = sbyte.MaxValue;
        card.SetCollisionLayer(2); // ensures that raycasts can no longer hit this card (so it can be cast *through* to where it might be dropped)
    }

    private void UpdateDragPosition(Vector2 screenPos)
    {
        if (dragged_card == null)
            return;

        var result = Raycast(screenPos);
        if (result == null)
            return;

        var (position, _) = result.Value;
        dragged_card.GlobalPosition = position + drag_offset;

        //hover_card?.HideBorder();
        hover_card = null;
    }

    private void TryEndDrag(Vector2 _)
    {
        if (dragged_card == null)
            return;

        dragged_card.SetCollisionLayer(1); // once no longer dragged, enable for raycasting again
        dragged_card = null;
        ResortCards(); // compress render order in groups of overlapping cards
    }

    private (Vector3, Card2)? Raycast(Vector2 screenPos)
    {
        var from = camera.ProjectRayOrigin(screenPos);
        var to = from + camera.ProjectRayNormal(screenPos) * 100;

        var query = PhysicsRayQueryParameters3D.Create(from, to);
        query.CollideWithAreas = true;
        query.CollideWithBodies = false;
        query.CollisionMask = 1;

        var cards = new List<Card2>();
        var position = (Vector3?)null;

        var result = GetWorld3D().DirectSpaceState.IntersectRay(query);
        // this loop grabs everything that a raycast can hit at this position, then returns the top-most card
        // at a minimum when over the table, this will result in two casts (one hits the table, one returns nothing)
        while (result.Count != 0)
        {
            position = (Vector3)result["position"];
            var area = result["collider"].As<Area3D>();
            if (area.GetParent() is Card2 c)
                cards.Add(c);

            var exclude = query.Exclude;
            exclude.Add(area.GetRid());
            query.Exclude = exclude;
            result = GetWorld3D().DirectSpaceState.IntersectRay(query);
        }

        if (!position.HasValue)
            return null;
        if (cards.Count == 0)
            return (position.Value, null);

        return (position.Value, cards.MaxBy(c => c.ZIndex));
    }

    public void ResortCards()
    {
        var n = cards.Count;
        var parent = new int[n];
        for (int i = 0; i < n; i++)
            parent[i] = i; // every element is in its own group, initially (its parent is itself, so its the root elem)

        Rect2 Rect(Card2 c) => new(new(c.Position.X, c.Position.Z), new Vector2(1, 1.5f));

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
        var groupsDict = new Dictionary<int, List<Card2>>();
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
                group[0].ZIndex = 0;
            else
                group.OrderBy(c => c.ZIndex).Select((card, index) => (card, index)).ToList().ForEach(o =>
                    o.card.ZIndex = (sbyte)(10 + (sbyte)o.index));
        }
    }
}
