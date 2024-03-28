using Mirror;
using UnityEngine;

// base class for all mobs, even Player
public abstract class Mob : NetworkBehaviour
{
    private float _invincible;
    protected int MaxHealth;
    protected int Health;
    protected int BasedDamage;

    public IBody Body;

    public int GetHealth() => MaxHealth;
    
    protected void InitMob(int mobName, IBody body)
    {
        gameObject.name = Game.Mobs.Names[mobName];
        MaxHealth = Game.Mobs.Health[mobName];
        Health = MaxHealth;
        Body = body;
    }

    protected void Damage(int amount)
    {
        if (Time.time - _invincible < Game.InvincibilityTime) return;
        Health = amount < Health ? Health - amount : 0;
        _invincible = Time.time;
        Body.Animator.ReceiveAnimation(Health == 0 ? AnimationType.Die : AnimationType.Hurt);
    }

    protected void Heal(int heal)
    {
        Health = MaxHealth > Health + heal ? Health + heal : MaxHealth;
    }
}

// all AI-controlled mobs need to implement this interface
public interface IAiControlled
{
    public void Init(int mobName);
    public Vector2 MoveFunction();
}

public class MobBopako : Mob, IAiControlled
{
    public void Init(int mobName)
    {
        // Body is a MobBody, meaning it will need a function to get the movement from
        InitMob(mobName, new AiBody(transform, MoveFunction));
    }

    void Update()
    {
        Body.Update();
    }

    public Vector2 MoveFunction()
    {
        return Vector2.zero;
    }
}

public class MobPokabo : Mob, IAiControlled
{
    public void Init(int mobName)
    {
        // Body is a MobBody, meaning it will need a function to get the movement from
        InitMob(mobName, new AiBody(transform, MoveFunction));
    }

    void Update()
    {
        Body.Update();
    }

    public Vector2 MoveFunction()
    {
        return Vector2.one;
    }
}

public class MobOakBoka : Mob, IAiControlled
{
    private bool _passif;
    public void Init(int mobName)
    {
        // pls, if someone read that, put a stupid condition so that the oak boka is not passive or passive
        _passif = true;
        
        // Body is a MobBody, meaning it will need a function to get the movement from
        InitMob(mobName, new AiBody(transform, MoveFunction));
    }

    void Update()
    {
        Body.Update();
    }

    public Vector2 MoveFunction()
    {
        return Vector2.zero;
    }
}

public class MobChefBoka : Mob, IAiControlled
{
    private bool _passif;
    public void Init(int mobName)
    {
        // pls, if someone read that, put a stupid condition so that the oak boka is not passive or passive
        _passif = true;
        
        // Body is a MobBody, meaning it will need a function to get the movement from
        InitMob(mobName, new AiBody(transform, MoveFunction));
    }

    void Update()
    {
        Body.Update();
    }

    public Vector2 MoveFunction()
    {
        return Vector2.zero;
    }
}