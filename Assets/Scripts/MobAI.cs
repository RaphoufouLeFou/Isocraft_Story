using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MobAI : MonoBehaviour
{
    private readonly Dictionary<string, float> _mobsHealth = new()
    {
        {"Zapatos(Clone)", 2},
        {"Mob(Clone)",9001}
    };

    private MobBody _body;

    private float _health;

    void Start()
    {
        _body = new MobBody(transform, MoveFunction);

        // initializing attached mob
        string gameObjectName = gameObject.name;
        _health = _mobsHealth[gameObjectName];
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
