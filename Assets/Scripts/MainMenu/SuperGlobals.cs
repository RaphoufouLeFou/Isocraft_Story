using System;
using UnityEngine;

public class SuperGlobals : MonoBehaviour
{
    // data that gets sent through scenes
    public static bool EditorMode = true; // set to false if started from main menu
    public static bool IsNewSave = true; // overriden if started from main menu

    // deprecated once game is loaded
    public static string SaveName;

    public static string PlayerName;

    // network data
    public static bool IsMultiplayerGame;
    public static bool IsHost;
    public static Uri Uri;

    public static void BackToMenu()
    {
        // reset cross-scene things, set up back to menu
        EditorMode = false;
        IsNewSave = true;
        SaveName = "";
        IsMultiplayerGame = false;
        IsHost = false;
    }
}
