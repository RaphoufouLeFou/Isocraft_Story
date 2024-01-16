using UnityEngine;
using TMPro;
using System;

public class Overlay : MonoBehaviour
{
    public TMP_Text textData;
    private float _displayFps, _displayMs;
    private long _lastUpdate;
    private const long UpdateDelay = 500; // ms

    void Update()
    {
        long now = DateTime.Now.Ticks / 10000;
        if (now >= _lastUpdate + UpdateDelay)
        {
            _displayFps = Time.deltaTime == 0 ? 1000000 : 1.0f / Time.deltaTime;
            _displayMs = Time.deltaTime * 1000;
            _lastUpdate = now;
        }

        string text = "";
        Vector3 pos = NetworkInfos.PlayerPos;
        if (Settings.OverlayParam.DisplayFps) text += "Fps: " + Round(_displayFps, 1) + "\n";
        if (Settings.OverlayParam.DisplayMs) text += "Last frame time: " + Round(_displayMs, 1) + "\n";
        if (Settings.OverlayParam.DisplayCoordinates)
            text += "Position: [" + Round(pos.x, 3) + ", " + Round(pos.y, 3) + ", " + Round(pos.z, 3) + "]\n";
        textData.text = text;
    }

    string Round(float n, int digits)
    {
        string s = (int)n + ".";
        if (n < 0) n = -n;
        for (int i = 0; i < digits; i++)
        {
            n -= (int)n;
            n *= 10;
            s += (int)n;
        }

        return s;
    }
}
