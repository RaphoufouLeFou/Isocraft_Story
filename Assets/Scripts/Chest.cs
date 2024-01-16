using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class Chest : MonoBehaviour
{
    public Animation animator1;
    public Animation animator2;

    public bool run;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (run)
        {
            animator1.Play();
            animator2.Play();
        }
    }
}
