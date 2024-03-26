using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobAI : MonoBehaviour
{
    private MobBody _body;

    // Start is called before the first frame update
    void Start()
    {
        _body = new MobBody(transform, MoveFunction);
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
