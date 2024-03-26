using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobAI : MonoBehaviour
{
    
    public CustomRigidBody Body;
    
    
    // Start is called before the first frame update
    void Start()
    {
        Body = new CustomRigidBody(transform, 8, 0.9f, 1.3f, -5, 0.95f, 1.85f);
    }

    // Update is called once per frame
    void Update()
    {
        Body.Update(Settings.IsPaused);
    }
}
