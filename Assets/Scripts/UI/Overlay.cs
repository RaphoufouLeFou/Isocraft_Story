using UnityEngine;
using TMPro;
using System;

public class Overlay : MonoBehaviour
{
    public TMP_Text textData;
    public GameObject game;
    private Game _game;
    private float _displayFps, _displayMs;
    private long _lastUpdate;
    private const long UpdateDelay = 500; // ms
    
    private void Start()
    {
        _game = game.GetComponent<Game>();
    }
    
    private void Update()
    {
        long now = DateTime.Now.Ticks / 10000;
        if (now >= _lastUpdate + UpdateDelay)
        {
            _displayFps = Time.deltaTime == 0 ? 1000000 : 1.0f / Time.deltaTime;
            _displayMs = Time.deltaTime * 1000;
            _lastUpdate = now;
        }

        string text = "";
        Vector3 pos = _game.player.transform.position;
        if (Settings.Overlay.DisplayFps) text += Round(_displayFps, 1) + " FPS" + (Settings.Overlay.DisplayMs ? " " : "\n");
        if (Settings.Overlay.DisplayMs) text += "(last: " + Round(_displayMs, 1) + "ms)\n";
        if (Settings.Overlay.DisplayCoords)
            text += "[" + Round(pos.x, 3) + ", " + Round(pos.y, 3) + ", " + Round(pos.z, 3) + "]\n";
        if (Settings.Overlay.DisplaySaveName)
            text += "Name : " + (_game.SaveManager.SaveName == "" ? "Unnamed" : _game.SaveManager.SaveName);
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
