using System.Collections.Generic;
using UnityEngine;

public static class Settings
{
    public static bool IsPaused;
    public static bool Playing; // true only if no popup is open, e.g. cancel place/break, don't move
    public static Dictionary<string, KeyCode> KeyMap;

    public struct OverlayStruct
    {
        public bool DisplayFps;
        public bool DisplayMs;
        public bool DisplayCoords;
        public bool DisplaySaveName;
    }

    public static OverlayStruct Overlay;

    public struct GameStruct
    {
        public bool FastGraphics;
        public int AutoSaveDelay;
        public int RenderDistance;
    }

    public static GameStruct Game;
}
