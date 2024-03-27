using System;
using Telepathy;
using UnityEngine;

public class MobAI : MonoBehaviour
{
    private MobBody _body;
    
    [NonSerialized] public float Health;
    

    public void SetName(int name)
    {
        _body = new MobBody(transform, MoveFunction);

        // initializing attached mob
        gameObject.name = Game.Mobs.Names[name];
        Health = Game.Mobs.Health[name];
        Debug.Log(Health);
    }

    void Start()
    {
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
