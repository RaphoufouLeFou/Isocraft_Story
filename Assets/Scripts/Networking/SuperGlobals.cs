using System;
using JetBrains.Annotations;
using UnityEngine;

public class SuperGlobals : MonoBehaviour
{
    // data that gets sent through scenes
    public static bool StartedFromMainMenu;
    public static bool IsNewSave = true; // overriden if started from main menu
    
    // deprecated once game is loaded
    [CanBeNull] private static string _saveName;
    public static string SaveName
    {
        get
        {
            string value = _saveName;
            if (value == null) Debug.LogWarning("Deprecated, use Game.SaveManager.SaveName instead");
            _saveName = null;
            return value;
        }
        set => _saveName = value;
    }

    // network data
    public static bool IsMultiplayerGame;
    public static bool IsHost;
    public static Uri Uri;

    public static void BackToMenu()
    {
        // reset cross-scene things like SaveName
        _saveName = "";
    }
}