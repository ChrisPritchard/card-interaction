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
        else if (@event is InputEventMouseButton touch && touch.ButtonIndex == MouseButton.Left)
        {
            if (touch.Pressed && dragged_card == null)
                TryStartDrag(touch.Position);
            else if (dragged_card != null)
                TryEndDrag(touch.Position);
        }
    }


    private void TryStartDrag(Vector2 screenPos)
    {
        var result = Raycast(screenPos);
        if (result == null)
            return;
        var (position, card) = result.Value;

        dragged_card = card;
        drag_start = card.GlobalPosition;
        drag_offset = card.GlobalPosition - position;
        card.Lift();
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

        var result = Raycast(screenPos);
        if (result == null)
            return;

        var (_, card) = result.Value;

        if (card != null)
            dragged_card.GlobalPosition = card.GlobalPosition + new Vector3(0, 0.05f, 0);
        else
            dragged_card.GlobalPosition = drag_start;

        dragged_card.Drop();
        dragged_card.SetCollisionLayer(1);

        dragged_card = null;
    }

    private (Vector3, Card)? Raycast(Vector2 screenPos)
    {
        var from = Camera.ProjectRayOrigin(screenPos);
        var to = from + Camera.ProjectRayNormal(screenPos) * 100;

        var query = PhysicsRayQueryParameters3D.Create(from, to);
        query.CollisionMask = 1 << 0;
        var result = GetWorld3D().DirectSpaceState.IntersectRay(query);

        if (result.Count == 0)
            return null;

        if (result["collider"].As<StaticBody3D>()?.Owner is Card card)
            return ((Vector3)result["position"], card);
        else
            return ((Vector3)result["position"], null);
    }
}
