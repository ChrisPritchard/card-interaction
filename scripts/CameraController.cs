using Godot;
using System;

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

    private Vector2 _touchStartPosition;
    private bool _isTouching = false;
    private Vector3 _currentVelocity = Vector3.Zero;
    private Vector3 _inputDirection = Vector3.Zero;

    public override void _Input(InputEvent @event)
    {
        // Handle touch input
        if (@event is InputEventScreenTouch touchEvent)
        {
            if (touchEvent.Pressed)
            {
                _touchStartPosition = touchEvent.Position;
                _isTouching = true;
            }
            else
            {
                _isTouching = false;
            }
        }
        else if (@event is InputEventScreenDrag dragEvent && _isTouching)
        {
            Vector2 dragDelta = (_touchStartPosition - dragEvent.Position) * TouchSensitivity;
            // For touch, we'll set the input direction directly
            _inputDirection = new Vector3(dragDelta.X, 0, dragDelta.Y).Normalized();
            _touchStartPosition = dragEvent.Position;
        }
    }

    public override void _Process(double delta)
    {
        HandleKeyboardInput();
        ApplyMovement((float)delta);
    }

    private void HandleKeyboardInput()
    {
        _inputDirection = Vector3.Zero;

        // WASD and Arrow keys input
        if (Input.IsActionPressed("ui_right"))
            _inputDirection.X += 1;
        if (Input.IsActionPressed("ui_left"))
            _inputDirection.X -= 1;
        if (Input.IsActionPressed("ui_down"))
            _inputDirection.Z += 1;
        if (Input.IsActionPressed("ui_up"))
            _inputDirection.Z -= 1;

        // Normalize diagonal movement
        if (_inputDirection.Length() > 0)
        {
            _inputDirection = _inputDirection.Normalized();
        }
    }

    private void ApplyMovement(float delta)
    {
        Vector3 targetVelocity = _inputDirection * MovementSpeed;

        // Apply acceleration or deceleration
        if (_inputDirection.Length() > 0)
        {
            // Accelerate towards target velocity
            _currentVelocity = _currentVelocity.Lerp(targetVelocity, Acceleration * delta);
        }
        else
        {
            // Decelerate to zero
            _currentVelocity = _currentVelocity.Lerp(Vector3.Zero, Deceleration * delta);

            // Stop completely when very close to zero to prevent tiny movements
            if (_currentVelocity.Length() < 0.1f)
            {
                _currentVelocity = Vector3.Zero;
            }
        }

        // Only move if we have meaningful velocity
        if (_currentVelocity.Length() > 0.01f)
        {
            MoveCamera(_currentVelocity * delta);
        }
    }

    private void MoveCamera(Vector3 movement)
    {
        if (movement.Length() == 0) return;

        Vector3 newPosition = GlobalPosition + movement;

        // Apply your specified limits
        newPosition.X = Mathf.Clamp(newPosition.X, XLimits.X, XLimits.Y);
        newPosition.Z = Mathf.Clamp(newPosition.Z, ZLimits.X, ZLimits.Y);

        GlobalPosition = newPosition;
    }
}