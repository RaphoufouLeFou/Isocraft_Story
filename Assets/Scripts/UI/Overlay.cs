using UnityEngine;
using TMPro;
using System;

public class Overlay : MonoBehaviour
{
    public TMP_Text textData;
    private float _displayFps, _displayMs;
    private long _lastUpdate;
    private const long UpdateDelay = 300; // ms

    private void Update()
    {
        if (!Game.Started) return;

        long now = DateTime.Now.Ticks / 10000;
        if (now >= _lastUpdate + UpdateDelay)
        {
            _displayFps = Time.deltaTime == 0 ? 1000000 : 1.0f / Time.deltaTime;
            _displayMs = Time.deltaTime * 1000;
            _lastUpdate = now;
        }

        string text = "";
        Vector3 pos = Vector3.zero;
        if (Game.Player != null)
             pos = Game.Player.transform.position;
        if (Settings.Overlay.DisplayFps)
            text += $"{_displayFps:F1} FPS {(Settings.Overlay.DisplayMs ? " " : "\n")}";
        if (Settings.Overlay.DisplayMs) text += $"(last: {_displayMs:F1}ms)\n";
        if (Settings.Overlay.DisplayCoords)
        {
            text += $"[{pos.x:F3}, {pos.y:F3}, {pos.z:F3}]\n";
            int rotStep = Utils.Mod((int)Game.Player.playerCamera.GoalRot.y, 360) / 45;
            string rot;
            switch (rotStep)
            {
                case 0: rot = "North (+Z)";
                    break;
                case 1: rot = "NorthEast (+X+Z)";
                    break;
                case 2: rot = "East (+X)";
                    break;
                case 3: rot = "SouthEast (+X-Z)";
                    break;
                case 4: rot = "South (-Z)";
                    break;
                case 5: rot = "SouthWest (-X-Z)";
                    break;
                case 6: rot = "West (-X)";
                    break;
                default: rot = "NorthWest (-X+Z)";
                    break;
            }

            text += $"Rotation: {rot}\n";
        }

        if (Settings.Overlay.DisplaySaveName)
            text += $"Save name: {(SuperGlobals.EditorMode ? "[editor mode]" : SuperGlobals.SaveName)}";
        textData.text = text;
    }
}
