using Godot;

public partial class Card : MeshInstance3D
{
    [Export] public StaticBody3D Body;

    private Tween tweener;

    const float lift_speed = 0.1f;
    const float lift_height = 0.05f;

    public void SetCollisionLayer(uint layer) => Body.CollisionLayer = (uint)(1 << (int)(layer - 1));

    public void Lift()
    {
        if (tweener != null && tweener.IsRunning())
            tweener.Kill();

        tweener = CreateTween();
        tweener.SetEase(Tween.EaseType.InOut);
        tweener.SetTrans(Tween.TransitionType.Quad);
        tweener.TweenProperty(this, "position:y", lift_height, lift_speed);
    }

    public void Drop()
    {
        if (tweener != null && tweener.IsRunning())
            tweener.Kill();

        tweener = CreateTween();
        tweener.SetEase(Tween.EaseType.InOut);
        tweener.SetTrans(Tween.TransitionType.Quad);
        tweener.TweenProperty(this, "position:y", 0, lift_speed);
    }
}
