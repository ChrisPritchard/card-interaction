using System;
using Godot;

public partial class Card : Sprite2D
{
    public TableScene Table { private get; set; }
    private bool dragged;
    private Vector2 offset;
    private int touchIndex = -1;

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouse && mouse.ButtonIndex == MouseButton.Left)
        {
            var rect = new Rect2(GlobalPosition, Texture.GetSize() * Scale);
            if (!rect.HasPoint(mouse.Position))
                return;

            if (mouse.Pressed)
                StartDrag(GetGlobalMousePosition());
            else
                dragged = false;
        }
        else if (@event is InputEventScreenTouch touch)
        {
            if (touch.Pressed && touch.Index != -1)
            {
                touchIndex = touch.Index;
                StartDrag(touch.Position);
            }
            else
            {
                touchIndex = -1;
                dragged = false;
            }
        }
        else if (dragged && @event is InputEventScreenDrag drag && drag.Index == touchIndex)
            Position = drag.Position + offset;
    }

    private void StartDrag(Vector2 start)
    {
        dragged = true;
        offset = Position - start;
        Table.BringToFront(this);
        GetViewport().SetInputAsHandled();
    }

    public override void _Process(double delta)
    {
        if (dragged && touchIndex == -1) // only for mouse drag here
            Position = GetGlobalMousePosition() + offset;
    }


}
