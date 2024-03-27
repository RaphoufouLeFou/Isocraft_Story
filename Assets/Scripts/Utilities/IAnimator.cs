public enum AnimationType
{
    Idle,
    Walk,
    Run,
    Jump,
    Attack,
    Hurt,
    Die
}

public interface IAnimator
{
    public void ReceiveAnimation(AnimationType type);
}

public class DummyAnimator : IAnimator
{
    public void ReceiveAnimation(AnimationType type) { }
}
