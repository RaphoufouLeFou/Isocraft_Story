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
    public void Animate(string animName);
}

public class PlayerAnimator : IAnimator
{
    private string _currentState;
    private readonly Animator _animator = Game.Player.transform.GetChild(1).GetComponent<Animator>();
    public void ReceiveAnimation(AnimationType type)
    {
        switch (type)
        {
            case AnimationType.Idle:
                Animate("Idle");
                break;
            case AnimationType.Walk:
                Animate("Walk");
                break;
            case AnimationType.Run:
                Animate("Sprint");
                break;
            case AnimationType.Jump:
                break;
            default:
                Animate("Idle");
                break;
        }
    }

    public void Animate(string animName)
    {
        if(_currentState == animName) return;
        _animator.Play(animName);
        _currentState = animName;
    }
}

public class MobAnimator : IAnimator
{
    private string _currentState;
    //private Animator _animator = Game.Player.transform.GetChild(1).GetComponent<Animator>();
    public void ReceiveAnimation(AnimationType type)
    {
        switch (type)
        {
            case AnimationType.Idle:
                Animate("Idle");
                break;
            case AnimationType.Walk:
                Animate("Walk");
                break;
            case AnimationType.Run:
                Animate("Sprint");
                break;
            case AnimationType.Jump:
                break;
            default:
                Animate("Idle");
                break;
        }
    }

    public void Animate(string animName)
    {
        /*
        if(_currentState == animName) return;
        _animator.Play(animName);
        _currentState = animName;
        */
    }
}

