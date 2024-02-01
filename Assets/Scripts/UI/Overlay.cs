using UnityEngine;
using TMPro;
using System;

public class Overlay : MonoBehaviour
{
    public TMP_Text textData;
    private float _displayFps, _displayMs;
    private long _lastUpdate;
    private const long UpdateDelay = 500; // ms
    private string _saveName;

    private void Start()
    {
        _saveName = SaveInfos.SaveName;
    }

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
        Vector3 pos = SaveInfos.PlayerPosition;
        if (Settings.Overlay.DisplayFps) text += Round(_displayFps, 1) + " FPS" + (Settings.Overlay.DisplayMs ? " " : "\n");
        if (Settings.Overlay.DisplayMs) text += "(last: " + Round(_displayMs, 1) + "ms)\n";
        if (Settings.Overlay.DisplayCoords)
            text += "[" + Round(pos.x, 3) + ", " + Round(pos.y, 3) + ", " + Round(pos.z, 3) + "]\n";
        if (Settings.Overlay.DisplaySaveName)
            text += "Name : " + (_saveName == "" ? "UnNamed" : _saveName);
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
