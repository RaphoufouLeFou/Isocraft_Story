using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEditor;

public static class Settings
{       
    public static bool IsPaused;
    public static Dictionary<string, KeyCode> KeyMap; // key: input name, value: keycode

    public struct OverlayStruct // overlay info struct
    {
        public bool DisplayFps; // displayed FPS
        public bool DisplayMs; // displayed ms per frame
        public bool DisplayCoords; //player coordinates
    }
    public static OverlayStruct Overlay; // overlay infos to be displayed
}

public class SettingsUI : MonoBehaviour
{
    // menu GameObjects
    public GameObject mainParamMenu;
    public GameObject mainParamMenuButtons;
    public GameObject overlayMenu;
    public GameObject controlsMenu;
    public GameObject inventoryMenu;
    public GameObject pauseMenu;
    public GameObject pressKeyText;
    public GameObject[] keysFields;

    private string _path;

    // variables when assigning a key
    private bool _isReadingKey; 
    private string _key;
    private TMP_Text _keyText;
    
    // unity button event functions
    public void ToggleFps() { Settings.Overlay.DisplayFps = !Settings.Overlay.DisplayFps; }
    public void ToggleMs() { Settings.Overlay.DisplayMs = !Settings.Overlay.DisplayMs; }
    public void ToggleCoordinates() { Settings.Overlay.DisplayCoords = !Settings.Overlay.DisplayCoords; }
    public void EnterOverlaySettings()
    {
        overlayMenu.SetActive(true); // show the overlay menu buttons
        mainParamMenuButtons.SetActive(false);  // hide the main settings buttons
    }
    public void EnterControlsSettings()
    {
        controlsMenu.SetActive(true); // show the controls menu buttons
        mainParamMenuButtons.SetActive(false);  // hide the main settings buttons
    }
    public void AssignKey(GameObject obj)
    {
        _isReadingKey = true;
        pressKeyText.SetActive(true); // show the "press a key" text
        _key = obj.name; // the function name is also the name of the UI input
        _keyText = obj.transform.GetChild(1).GetComponentInChildren<TMP_Text>(); // get the pressed key Text to hide it later
    }
    public void ButtonResumeClick()
    {
        // hide the pause menu
        Settings.IsPaused = !Settings.IsPaused;
        pauseMenu.SetActive(Settings.IsPaused);
        SaveSettings();
    }
    
    public void BackToMainSettings()
    {
        // go back button in sub-settings
        mainParamMenu.SetActive(true);
        // all sub-settings
        overlayMenu.SetActive(false);
        controlsMenu.SetActive(false);
        // also called by the settings button in the pause menu, so we need to hide it
        pauseMenu.SetActive(false);
        // show the settings buttons
        mainParamMenuButtons.SetActive(true);
    }

    public void BackToPauseMenu()
    {
        // go back to main settings
        mainParamMenu.SetActive(false);
        pauseMenu.SetActive(true);
        mainParamMenuButtons.SetActive(false);
    }

    private void SaveSettings()
    {
        string s = "Ms:" + Settings.Overlay.DisplayMs + "\n";
        s += "Fps:" + Settings.Overlay.DisplayFps + "\n";
        s += "Coords:" + Settings.Overlay.DisplayCoords + "\n";
        foreach (KeyValuePair<string, KeyCode> key in Settings.KeyMap) s += key.Key + ":" + (int)key.Value + "\n";
        if (File.Exists(_path)) File.Delete(_path);
        File.WriteAllText(_path, s);
    }

    private void UpdateSceneSettings()
    {
        GameObject go = GameObject.Find("AssignContent");
        for (int i = 0; i < Settings.KeyMap.Count; i++)
        {
            Transform child = go.transform.GetChild(i);
            child.GetChild(1).GetComponentInChildren<TMP_Text>().text = Settings.KeyMap[child.gameObject.name].ToString();
        }
    }
    
    private void LoadSettings()
    {
        // set all settings to default
        Settings.Overlay.DisplayMs = true;
        Settings.Overlay.DisplayFps = true;
        Settings.Overlay.DisplayCoords = true;

        Settings.KeyMap = new Dictionary<string, KeyCode>
        {
            { "Forwards", KeyCode.W },
            { "Backwards", KeyCode.S },
            { "Left", KeyCode.A },
            { "Right", KeyCode.D },
            { "CamLeft", KeyCode.Q },
            { "CamRight", KeyCode.E },
            { "Kill", KeyCode.K },
            { "TopView", KeyCode.T },
            { "Inventory", KeyCode.Tab },
            { "Respawn", KeyCode.R }
        };

        string[] keys =
        {
            "Ms", "Fps", "Coords", "Forwards", "Backwards", "Left", "Right", "CamLeft", "CamRight", "Kill", "TopView",
            "Inventory", "Respawn"
        }; // correct keys in save file

        // replace by existing settings in file
        if (File.Exists(_path))
        {
            StreamReader file = new StreamReader(_path);
            while (file.ReadLine() is { } line)
            {
                // check if valid line and edit settings
                int i;
                for (i = 0; i < line.Length; i++) if (line[i] == ':') break;
                if (i == line.Length) continue;
                string key = line.Substring(0, i), value = line.Substring(i + 1);
                if (!keys.Contains(key)) continue;
                if (key == "Ms") Settings.Overlay.DisplayMs = value == "True";
                else if (key == "Fps") Settings.Overlay.DisplayFps = value == "True";
                else if (key == "Coords") Settings.Overlay.DisplayCoords = value == "True";
                else if (int.TryParse(value, out i)) Settings.KeyMap[key] = (KeyCode)i;
            }
            file.Close();
        }

        // update fields in scene
        UpdateSceneSettings();
        // save fixed settings to file
        SaveSettings();
    }

    private void Start()
    {
        _path = Application.persistentDataPath + "/options.txt";
        // set all settings to hidden
        pauseMenu.SetActive(false);
        mainParamMenu.SetActive(false);
        controlsMenu.SetActive(false);
        mainParamMenuButtons.SetActive(false);
        pressKeyText.SetActive(false);
        overlayMenu.SetActive(false);
        LoadSettings();
    }

    private void Update()
    {
        if (!_isReadingKey && Input.GetKeyDown(KeyCode.Escape)) // if the escape key is pressed but not while assigning a key
            if (pauseMenu.activeSelf || (!Settings.IsPaused && !inventoryMenu.activeSelf)) // toggle the pause menu except the inventory is shown
            {
                Settings.IsPaused = !Settings.IsPaused;
                pauseMenu.SetActive(Settings.IsPaused);
                if (Settings.IsPaused == false) SaveSettings(); // save the settings
            } else if (mainParamMenuButtons.activeSelf) BackToPauseMenu();
            else if (
                overlayMenu.activeSelf 
                || controlsMenu.activeSelf
                // otherMenu parameters
                // ...
            ) BackToMainSettings(); // get back of one setting window
        
        if (!_isReadingKey) return; // if is not in key assign mode, the update is done

        if (Input.anyKey) // if a key is detected
            foreach(KeyCode key in Enum.GetValues(typeof(KeyCode))) // enum in all possible keys
                if (Input.GetKey(key))
                {
                    _isReadingKey = false;
                    pressKeyText.SetActive(_isReadingKey); //hide the text
                    
                    if(key == KeyCode.Escape) return; // cancel if escape key is pressed
                    
                    Settings.KeyMap[_key] = key; // map the key in the dictionary
                    _keyText.text = key.ToString();  // update the button text of the parameter 
                }
    }
}