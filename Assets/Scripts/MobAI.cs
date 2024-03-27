using System;
using UnityEngine;

public class MobAI : MonoBehaviour
{
    private MobBody _body;

    private int _name;
    [NonSerialized] public float Health;

    public MobAI(int name)
    {
        _name = name;
    }

    void Start()
    {
        _body = new MobBody(transform, MoveFunction);

        // initializing attached mob
        gameObject.name = Game.Mobs.Names[_name];
        Health = Game.Mobs.Health[_name];
    }

    void Update()
    {
        _body.Update();
    }

    private (float side, float forwards) MoveFunction()
    {
        // should return movement relative to rotation
        return (0, 0);
    }
}
