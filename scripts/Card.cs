using System;
using Godot;

public partial class Card : Sprite2D
{
    [Export] public Shader BorderShader { get; set; }
    public Table Table { private get; set; }

    private ShaderMaterial shaderMaterial;
    private bool mouse_over = false;

    private bool dragged;
    private Vector2 offset;
    private int touchIndex = -1;

    public override void _Ready()
    {
        shaderMaterial = new ShaderMaterial
        {
            Shader = BorderShader
        };
        Material = shaderMaterial;
    }

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

            GetViewport().SetInputAsHandled();
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

            GetViewport().SetInputAsHandled();
        }

        if (dragged && @event is InputEventScreenDrag drag && drag.Index == touchIndex)
        {
            Position = drag.Position + offset;

            GetViewport().SetInputAsHandled();
        }

        if (@event is InputEventMouseMotion move)
        {
            var mousePos = GetGlobalMousePosition();
            var rect = new Rect2(GlobalPosition, Texture.GetSize() * Scale);

            var nowOver = rect.HasPoint(mousePos);
            if (nowOver && !mouse_over)
            {
                CreateTween()
                    .TweenMethod(Callable.From<float>(v => shaderMaterial.SetShaderParameter("fade_amount", v)), 0.0f, 1.0f, 0.3f)
                    .SetEase(Tween.EaseType.Out)
                    .SetTrans(Tween.TransitionType.Sine);
                mouse_over = true;
            }
            else if (!nowOver && mouse_over)
            {
                CreateTween()
                    .TweenMethod(Callable.From<float>(v => shaderMaterial.SetShaderParameter("fade_amount", v)), 1.0f, 0.0f, 0.3f)
                    .SetEase(Tween.EaseType.Out)
                    .SetTrans(Tween.TransitionType.Sine);
                mouse_over = false;
            }

            GetViewport().SetInputAsHandled();
        }
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
