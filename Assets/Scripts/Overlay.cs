using UnityEngine;
using TMPro;
using System;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Overlay : MonoBehaviour
{
    [FormerlySerializedAs("TextData")] public TMP_Text textData;
    [NonSerialized] public float FPS;
    [NonSerialized] public float MS;
    [FormerlySerializedAs("UpdatePerSecond")] public int updatePerSecond = 2;
    private int _itterations = 1;

    void Update()
    {
        if(_itterations >= FPS / updatePerSecond)
        {
            FPS = 1.0f / Time.deltaTime;
            MS = Time.deltaTime * 1000.0f;
            string text = $"Fps : {FPS}\nLast frame time : {MS} ms";
            textData.text = text;
            _itterations = 0;
        }
        else
            _itterations++;
    }
}
