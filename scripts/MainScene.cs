using Godot;

public partial class MainScene : Node3D
{
    [Export] public PackedScene CardScene { get; set; }
    [Export] public Camera3D Camera;
}
