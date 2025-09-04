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
        shaderMaterial.SetShaderParameter("fade_amount", 0f);
        Material = shaderMaterial;
    }

    public override void _Input(InputEvent @event)
    {
        var rect = new Rect2(GlobalPosition, Texture.GetSize() * Scale);
        var position = (@event as InputEventMouse)?.Position ?? (@event as InputEventScreenDrag)?.Position ?? (@event as InputEventScreenTouch)?.Position;
        if (position == null || !rect.HasPoint(position.Value))
        {
            if (mouse_over)
            {
                mouse_over = false;
                FadeOut();
            }
            return;
        }

        if (@event is InputEventMouseButton mouse)
        {
            if (!dragged && mouse.ButtonIndex == MouseButton.Left && mouse.Pressed)
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
        else if (!mouse_over)
        {
            mouse_over = true;
            FadeIn();
            GetViewport().SetInputAsHandled();
        }
    }

    private void FadeIn() => CreateTween()
                .TweenMethod(Callable.From<float>(v => shaderMaterial.SetShaderParameter("fade_amount", v)), 0.0f, 1.0f, 0.3f)
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Sine);

    private void FadeOut() => CreateTween()
                .TweenMethod(Callable.From<float>(v => shaderMaterial.SetShaderParameter("fade_amount", v)), 1.0f, 0.0f, 0.3f)
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Sine);

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
