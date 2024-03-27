using Mirror;
using UnityEngine;

public class Mob : NetworkBehaviour
{
    private float _invincible;
    protected int MaxHealth;
    protected int Health;

    public IBody Body;

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
    }
}

// mob child, but with AI capabilities
public class MobAI : Mob
{
    public void Init(int mobName)
    {
        InitMob(mobName, new MobBody(transform, MoveFunction));
        Body.MovementType = BodyMovement.Absolute;
    }

    void Update()
    {
        Body.Update();
    }

    private (float side, float forwards) MoveFunction()
    {
        return (0, 0);
    }
}
