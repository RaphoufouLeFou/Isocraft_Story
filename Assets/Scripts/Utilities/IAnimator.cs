
using UnityEngine;

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
    private string _currentState;
    private Animator _animator = Game.Player.transform.GetChild(1).GetComponent<Animator>();
    public void ReceiveAnimation(AnimationType type)
    {
        switch (type)
        {
            case AnimationType.Idle:
                Annimate("Idle");
                break;
            case AnimationType.Walk:
                Annimate("Walk");
                break;
            case AnimationType.Run:
                Annimate("Sprint");
                break;
            case AnimationType.Jump:
                break;
            default:
                Annimate("Idle");
                break;
        }
    }

    private void Annimate(string animName)
    {
        if(_currentState == animName) return;
        _animator.Play(animName);
        _currentState = animName;
    }
}
