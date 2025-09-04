using Godot;

public partial class MainScene : Node3D
{
    [Export] public Sprite3D TableSprite;
    [Export] public SubViewport TableViewPort;
    [Export] public BoxShape3D TableCollisionMesh;
    [Export] public TableScene Table;
    [Export] public Camera3D Camera;

    public override void _Input(InputEvent @event)
    {
        var table_pos = GetTablePosition(@event);
        if (table_pos != null)
        {
            if (@event is InputEventMouseButton mouse && mouse.Pressed)
            {
                GD.Print("pressed at " + table_pos);
            }
            var transformedEvent = @event.Duplicate() as InputEvent;
            ApplyPositionToEvent(transformedEvent, table_pos.Value);
            TableViewPort.PushInput(transformedEvent, true);
            // Table._Input(transformedEvent);
            GetViewport().SetInputAsHandled();
        }
    }

    private Vector2? GetTablePosition(InputEvent @event)
    {
        Vector2 screenPoint;
        if (@event is InputEventMouse)
            screenPoint = GetViewport().GetMousePosition();
        else if (@event is InputEventScreenTouch touch)
            screenPoint = touch.Position;
        else
            return null;

        var from = Camera.ProjectRayOrigin(screenPoint);
        var to = from + Camera.ProjectRayNormal(screenPoint) * 1000;

        var space = GetWorld3D().DirectSpaceState;
        var query = PhysicsRayQueryParameters3D.Create(from, to);
        query.CollideWithAreas = true;
        query.CollideWithBodies = false;
        query.CollisionMask = 1;

        var result = space.IntersectRay(query);
        if (result.Count == 0)
            return null;

        var area3d = result["collider"].As<Area3D>();
        if (area3d == null || area3d.GetParent() != TableSprite)
            return null;

        var localIntersection = area3d.ToLocal(result["position"].As<Vector3>());
        var mesh_size = TableCollisionMesh.Size;
        var table_size = TableViewPort.Size;

        var uv = new Vector2((localIntersection.X + mesh_size.X / 2) / mesh_size.X, (localIntersection.Z + mesh_size.Z / 2) / mesh_size.Z);
        var table_pos = new Vector2(uv.X * table_size.X - table_size.X / 2, uv.Y * table_size.Y - table_size.Y / 2);

        return table_pos;
    }

    private static void ApplyPositionToEvent(InputEvent @event, Vector2 position)
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
