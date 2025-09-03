using Godot;

public partial class MainScene : Node3D
{
    [Export] public Sprite3D TableSprite;
    [Export] public SubViewport TableViewPort;
    [Export] public Camera3D Camera;

    public override void _Input(InputEvent @event)
    {
        if (IsEventOnTable(@event))
        {
            var transformedEvent = TransformEventToViewport(@event);
            TableViewPort.PushInput(transformedEvent);
            GetViewport().SetInputAsHandled();
        }
    }


    private bool IsEventOnTable(InputEvent @event)
    {
        if (@event is InputEventMouse)
        {
            if (@event is InputEventMouseButton btn && btn.Pressed)
            {
                GD.Print("pressed");
            }
            var mousePos = GetViewport().GetMousePosition();
            return IsPointOnTable(mousePos);
        }
        else if (@event is InputEventScreenTouch touchEvent)
            return IsPointOnTable(touchEvent.Position);

        return false;
    }

    private bool IsPointOnTable(Vector2 screenPoint)
    {
        var from = Camera.ProjectRayOrigin(screenPoint);
        var to = from + Camera.ProjectRayNormal(screenPoint) * 1000;

        var space = GetWorld3D().DirectSpaceState;
        var query = PhysicsRayQueryParameters3D.Create(from, to);
        query.CollideWithAreas = true;
        query.CollideWithBodies = false;
        query.CollisionMask = 1;

        var result = space.IntersectRay(query);

        if (result.Count > 0 && result["collider"].As<Area3D>().GetParent() == TableSprite)
            return true;

        return false;
    }

    private InputEvent TransformEventToViewport(InputEvent @event)
    {
        var transformedEvent = @event.Duplicate() as InputEvent;
        var screenPoint = Vector2.Zero;

        if (transformedEvent is InputEventMouse)
        {
            screenPoint = GetViewport().GetMousePosition();
        }
        else if (transformedEvent is InputEventScreenTouch touchEvent)
        {
            screenPoint = touchEvent.Position;
        }
        else if (transformedEvent is InputEventScreenDrag dragEvent)
        {
            screenPoint = dragEvent.Position;
        }

        var uv = GetSprite3DUV(screenPoint);
        if (uv == Vector2.Zero)
            return transformedEvent;

        var viewportPos = new Vector2(
            uv.X * TableViewPort.Size.X,
            uv.Y * TableViewPort.Size.Y
        );

        ApplyPositionToEvent(transformedEvent, viewportPos);
        return transformedEvent;
    }

    private Vector2 GetSprite3DUV(Vector2 screenPoint)
    {
        var from = Camera.ProjectRayOrigin(screenPoint);
        var to = from + Camera.ProjectRayNormal(screenPoint) * 1000;

        var space = GetWorld3D().DirectSpaceState;
        var query = PhysicsRayQueryParameters3D.Create(from, to);
        query.CollideWithAreas = true;
        query.CollideWithBodies = false;
        query.CollisionMask = 1;

        var result = space.IntersectRay(query);

        if (result.Count > 0 && result["collider"].As<Area3D>().GetParent() == TableSprite)
        {
            var localIntersection = TableSprite.ToLocal(result["position"].As<Vector3>());

            float u = localIntersection.X + 0.5f;
            float v = localIntersection.Z + 0.5f;

            return new Vector2(u, v);
        }

        return Vector2.Zero;
    }

    private void ApplyPositionToEvent(InputEvent @event, Vector2 position)
    {
        if (@event is InputEventMouse mouseEvent)
        {
            if (mouseEvent is InputEventMouseButton mouseButton)
            {
                mouseButton.Position = position;
                mouseButton.GlobalPosition = position;
            }
            else if (mouseEvent is InputEventMouseMotion mouseMotion)
            {
                mouseMotion.Position = position;
                mouseMotion.GlobalPosition = position;
            }
        }
        else if (@event is InputEventScreenTouch touchEvent)
        {
            touchEvent.Position = position;
        }
        else if (@event is InputEventScreenDrag dragEvent)
        {
            dragEvent.Position = position;
        }
    }

}
