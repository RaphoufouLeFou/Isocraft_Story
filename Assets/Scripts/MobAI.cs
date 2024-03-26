using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MobAI : MonoBehaviour
{
    private Dictionary<string, float> dico = new()
    {
        {"Zapatos(Clone)", 2},
        {"Mob(Clone)",9001}
    };
    
    private MobBody _body;

    private float Health;
    
    
    // Start is called before the first frame update
    void Start()
    {
        _body = new MobBody(transform, MoveFunction);
        
        
        // Accède au Mob attaché et initialisation des différentes caractéristiques du mob.
        string gameObjectName = gameObject.name;
        Health = dico[gameObjectName];
    }

    // Update is called once per frame
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
