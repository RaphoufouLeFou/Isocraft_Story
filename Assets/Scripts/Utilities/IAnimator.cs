
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
                Debug.Log("Idling");
                Annimate("Idle");
                break;
            case AnimationType.Walk:
                Debug.Log("walking");
                Annimate("Walk");
                break;
            case AnimationType.Run:
                Debug.Log("Sprinting");
                Annimate("Sprint");
                break;
            case AnimationType.Jump:
                break;
            default:
                Debug.Log("Idling from somewhere else");
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
