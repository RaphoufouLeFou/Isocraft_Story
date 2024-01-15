using UnityEngine;
using TMPro;
using System;
using UnityEngine.Serialization;
using Mirror;

public class Overlay : MonoBehaviour
{
    [FormerlySerializedAs("TextData")] public TMP_Text textData;
    [NonSerialized] public float FPS;
    [NonSerialized] public float MS;
    [FormerlySerializedAs("UpdatePerSecond")] public int updatePerSecond = 2;
    private int _iterations = 1;

    void Update()
    {
        if(_iterations >= FPS / updatePerSecond)
        {
            FPS = 1.0f / Time.deltaTime;
            MS = Time.deltaTime * 1000.0f;
            _iterations = 0;
        }
        else _iterations++;
        string text = "";
        Vector3 pos = NetworkInfos.PlayerPos;
        if (Settings.OverlayParam.DisplayFps) text += "Fps: " + Round(FPS, 1) + "\n";
        if (Settings.OverlayParam.DisplayMspf) text += "Last frame time: " + Round(MS, 1) + "\n";
        if (Settings.OverlayParam.DisplayCoordinates)
            text += "Position: [" + Round(pos.x, 3) + ", " + Round(pos.y, 3) + ", " + Round(pos.z, 3) + "]\n";
        textData.text = text;
    }

    string Round(float n, int digits)
    {
        string s = (int)n + ".";
        for (int i = 0; i < digits; i++)
        {
            n -= (int)n;
            n *= 10;
            s += (int)n;
        }

        return s;
    }
}
