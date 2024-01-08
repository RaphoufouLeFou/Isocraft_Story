using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class Overlay : MonoBehaviour
{
    public TMP_Text TextData;
    [NonSerialized] public float fps = 0;
    [NonSerialized] public float ms;
    public int UpdatePerSecond = 2;
    private int itterations = 1;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(itterations >= fps / UpdatePerSecond)
        {
            fps = 1.0f / Time.deltaTime;
            ms = Time.deltaTime * 1000.0f;
            string text = $"Fps : {fps}\nLast frame time : {ms} ms";
            TextData.text = text;
            itterations = 0;
        }
        else
            itterations++;
    }
}
