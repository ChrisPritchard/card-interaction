using Godot;

public partial class CameraController : Camera3D
{
    [Export]
    public float MovementSpeed { get; set; } = 10.0f;

    [Export]
    public float TouchSensitivity { get; set; } = 0.01f;

    [Export]
    public float Acceleration { get; set; } = 8.0f;

    [Export]
    public float Deceleration { get; set; } = 10.0f;

    [Export]
    public Vector2 XLimits { get; set; } = new Vector2(-5, 5);

    [Export]
    public Vector2 ZLimits { get; set; } = new Vector2(-3, 10);

    private Vector2 touchStartPosition;
    private bool isTouching = false;
    private Vector3 currentVelocity = Vector3.Zero;
    private Vector3 inputDirection = Vector3.Zero;

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventScreenTouch touchEvent)
        {
            if (touchEvent.Pressed)
            {
                touchStartPosition = touchEvent.Position;
                isTouching = true;
            }
            else
            {
                isTouching = false;
            }
        }
        else if (@event is InputEventScreenDrag dragEvent && isTouching)
        {
            Vector2 dragDelta = (touchStartPosition - dragEvent.Position) * TouchSensitivity;
            inputDirection = new Vector3(dragDelta.X, 0, dragDelta.Y).Normalized();
            touchStartPosition = dragEvent.Position;
        }
    }

    public override void _Process(double delta)
    {
        HandleKeyboardInput();
        ApplyMovement((float)delta);
    }

    private void HandleKeyboardInput()
    {
        inputDirection = Vector3.Zero;

        if (Input.IsActionPressed("ui_right"))
            inputDirection.X += 1;
        if (Input.IsActionPressed("ui_left"))
            inputDirection.X -= 1;
        if (Input.IsActionPressed("ui_down"))
            inputDirection.Z += 1;
        if (Input.IsActionPressed("ui_up"))
            inputDirection.Z -= 1;

        if (inputDirection.Length() > 0)
            inputDirection = inputDirection.Normalized();
    }

    private void ApplyMovement(float delta)
    {
        var targetVelocity = inputDirection * MovementSpeed;

        if (inputDirection.Length() > 0)
        {
            currentVelocity = currentVelocity.Lerp(targetVelocity, Acceleration * delta);
        }
        else
        {
            currentVelocity = currentVelocity.Lerp(Vector3.Zero, Deceleration * delta);
            if (currentVelocity.Length() < 0.1f)
            {
                currentVelocity = Vector3.Zero;
            }
        }

        if (currentVelocity.Length() > 0.01f)
        {
            MoveCamera(currentVelocity * delta);
        }
    }

    private void MoveCamera(Vector3 movement)
    {
        if (movement.Length() == 0)
            return;

        Vector3 newPosition = GlobalPosition + movement;

        newPosition.X = Mathf.Clamp(newPosition.X, XLimits.X, XLimits.Y);
        newPosition.Z = Mathf.Clamp(newPosition.Z, ZLimits.X, ZLimits.Y);

        GlobalPosition = newPosition;
    }
}