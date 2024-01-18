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
        if (Settings.Overlay.DisplayFps) text += Round(_displayFps, 1) + " FPS";
        if (Settings.Overlay.DisplayMs) text += " (last: " + Round(_displayMs, 1) + "ms)";
        if (Settings.Overlay.DisplayCoords)
            text += "\n[" + Round(pos.x, 3) + ", " + Round(pos.y, 3) + ", " + Round(pos.z, 3) + "]";
        text += "\nName : " + SaveInfos.SaveName;
        textData.text = text;
    }

    string Round(float n, int digits)
    {
        if (digits == 0) return (int)n + "";
        string s = (int)Math.Abs(n) + ".";

        if (n < 0)
        {
            s = "-" + s;
            n = -n;
        }
        for (int i = 0; i < digits; i++)
        {
            n -= (int)n;
            n *= 10;
            s += (int)n;
        }

        return s;
    }
}
