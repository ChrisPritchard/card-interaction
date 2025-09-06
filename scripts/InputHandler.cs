using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Godot;

public partial class InputHandler : Node3D
{
    [Export] public Camera3D Camera { get; set; }

    private Card dragged_card;
    private Vector3 drag_offset;
    private Vector3 drag_start;

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion move && dragged_card != null)
            UpdateDragPosition(move.Position);
        else if (@event is InputEventMouseButton click && click.ButtonIndex == MouseButton.Left)
        {
            if (click.Pressed && dragged_card == null)
                TryStartDrag(click.GlobalPosition);
            else if (dragged_card != null)
                TryEndDrag(click.GlobalPosition);
        }
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
        card.SetCollisionLayer(3);
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
    }

    private void TryEndDrag(Vector2 screenPos)
    {
        if (dragged_card == null)
            return;

        // var result = Raycast(screenPos);
        // if (result == null)
        //     return;

        // var (_, card) = result.Value;

        // if (card != null)
        //     dragged_card.GlobalPosition = card.GlobalPosition + new Vector3(0, 0.02f, 0);

        dragged_card.SetCollisionLayer(1);
        dragged_card = null;
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

        return (position.Value, cards.MaxBy(c => c.Depth));
    }
}
