using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class InputHandler : Node3D
{
    [Export] public MainScene Main { get; set; }
    [Export] public Camera3D Camera { get; set; }

    private Card dragged_card;
    private Card hover_card;
    private Card last_hover_card;
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
            hover_card?.HideBorder();
            hover_card = null;
            return;
        }

        if (card == hover_card || card == last_hover_card)
            return;

        if (hover_card != null)
            hover_card?.HideBorder();

        last_hover_card = card; // can't hover over this card again until another card or empty space is encountered
        hover_card = card;
        card.ShowBorder();
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
        card.RenderOrder = sbyte.MaxValue;
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

        hover_card?.HideBorder();
        hover_card = null;
    }

    private void TryEndDrag(Vector2 screenPos)
    {
        if (dragged_card == null)
            return;

        dragged_card.SetCollisionLayer(1); // once no longer dragged, enable for raycasting again
        dragged_card = null;
        Main.ReSortCards(); // compress render order in groups of overlapping cards
    }

    private (Vector3, Card)? Raycast(Vector2 screenPos)
    {
        var from = Camera.ProjectRayOrigin(screenPos);
        var to = from + Camera.ProjectRayNormal(screenPos) * 100;

        var query = PhysicsRayQueryParameters3D.Create(from, to);
        query.CollideWithAreas = true;
        query.CollideWithBodies = false;
        query.CollisionMask = 1;

        var cards = new List<Card>();
        var position = (Vector3?)null;

        var result = GetWorld3D().DirectSpaceState.IntersectRay(query);
        // this loop grabs everything that a raycast can hit at this position, then returns the top-most card
        // at a minimum when over the table, this will result in two casts (one hits the table, one returns nothing)
        while (result.Count != 0)
        {
            position = (Vector3)result["position"];
            var area = result["collider"].As<Area3D>();
            if (area.GetParent() is Card c)
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

        return (position.Value, cards.MaxBy(c => c.RenderOrder));
    }
}
