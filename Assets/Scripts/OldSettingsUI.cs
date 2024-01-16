using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public static class OldSettings
{       
    public static bool IsPaused;
    public static Dictionary<string, KeyCode> KeyMap = new(); // key: input name, value: keycode
    public struct Overlay // overlay info struct
    {
        public bool DisplayFps; // displayed FPS
        public bool DisplayMs; // displayed ms per frame
        public bool DisplayCoordinates; //player coordinates
    }

    public static Overlay OverlayParam; // overlay infos to be displayed
}

// settings formatted to be understood by Unity's json converter
public class OldSaveSettings
{       
    private List<string> _keyMapListStr;
    private List<KeyCode> _keyMapListKeys;
    private bool _isPaused;
    private bool _displayFps;
    private bool _displayMs;
    private bool _displayCoordinates;
    
    public void InitParam()
    {
        //Convert the dictionary to two lists for the json
        _keyMapListStr = new List<string>();
        _keyMapListKeys = new List<KeyCode>();
        foreach (KeyValuePair<string,KeyCode> hKeyValuePair in Settings.KeyMap)
        {
            _keyMapListStr.Add(hKeyValuePair.Key);
            _keyMapListKeys.Add(hKeyValuePair.Value);
        }
        // set the values to be saved
        _isPaused = Settings.IsPaused;
        _displayMs = Settings.OverlayParam.DisplayMs;
        _displayFps = Settings.OverlayParam.DisplayFps;
        _displayCoordinates = Settings.OverlayParam.DisplayCoords;
    }
    public void RestoreParam()
    {
        // convert the thw saved lists back to the dictionary
        Settings.KeyMap = new Dictionary<string, KeyCode>();
        for (int i = 0; i < _keyMapListStr.Count; i++) Settings.KeyMap.Add(_keyMapListStr[i], _keyMapListKeys[i]);
        
        // restore the saved values
        Settings.IsPaused = _isPaused;
        Settings.OverlayParam.DisplayMs = _displayMs;
        Settings.OverlayParam.DisplayFps = _displayFps;
        Settings.OverlayParam.DisplayCoords = _displayCoordinates;
    }
}

public class OldSettingsUI : MonoBehaviour
{
    // menu GameObjects
    public GameObject mainParamMenu;
    public GameObject mainParamMenuButtons;
    public GameObject overlayMenu;
    public GameObject controlsMenu;
    public GameObject inventoryMenu;
    public GameObject pauseMenu;
    public GameObject pressKeyText;

    // variables when assigning a key
    private bool _isReadingKey; 
    private string _function;
    private TMP_Text _keyText;
    
    void Start()
    {
        // set all settings to hidden
        pauseMenu.SetActive(false);
        mainParamMenu.SetActive(false);
        controlsMenu.SetActive(false);
        mainParamMenuButtons.SetActive(false);
        pressKeyText.SetActive(false);
        overlayMenu.SetActive(false);
        LoadSettings();
    }

    public void ToggleFps()
    {
        Settings.OverlayParam.DisplayFps = !Settings.OverlayParam.DisplayFps;
    }
    public void ToggleMs()
    {
        Settings.OverlayParam.DisplayMs = !Settings.OverlayParam.DisplayMs;
    }
    public void ToggleCoordinates()
    {
        Settings.OverlayParam.DisplayCoords = !Settings.OverlayParam.DisplayCoords;
    }
    public void EnterOverlayParam()
    {
        overlayMenu.SetActive(true); // show the overlay menu buttons
        mainParamMenuButtons.SetActive(false);  // hide the main settings buttons
    }
    
    public void EnterControlsParam()
    {
        controlsMenu.SetActive(true); // show the controls menu buttons
        mainParamMenuButtons.SetActive(false);  // hide the main settings buttons
    }

    public void ReturnToMainParam()
    {
        // do back button in sub-settings
        mainParamMenu.SetActive(true);
        // all sub-settings
        overlayMenu.SetActive(false);
        controlsMenu.SetActive(false);
        // also called by the settings button in the pause menu, so we need to hide it
        pauseMenu.SetActive(false);
        // show the settings buttons
        mainParamMenuButtons.SetActive(true);
    }

    public void ReturnToPauseMenu()
    {
        // go back to main settings
        mainParamMenu.SetActive(false);
        pauseMenu.SetActive(true);
        mainParamMenuButtons.SetActive(false);
    }

    public void AssignKey(GameObject function)
    {
        _isReadingKey = true;
        pressKeyText.SetActive(_isReadingKey); // show the "press a key" text
        _function = function.name; // the function name is also the name of the UI input
        _keyText = function.transform.GetChild(1).GetComponentInChildren<TMP_Text>(); // get the press Key Text to hide it later
    }
   
    public void ButtonResumeClick()
    {
        // hide the pause menu
        Settings.IsPaused = !Settings.IsPaused;
        pauseMenu.SetActive(Settings.IsPaused);
        SaveSettings();
    }

    private void SaveSettings()
    {
        // save the settings into a json file in the AppData folder
        OldSaveSettings saveSettings = new OldSaveSettings();
        saveSettings.InitParam();
        string jsonSave = JsonUtility.ToJson(saveSettings, true);
        string path = Application.persistentDataPath + "/Settings.json";
        if (File.Exists(path)) File.Delete(path);
        File.WriteAllText(path, jsonSave);
    }

    private void LoadSettings()
    {
        // read the settings from the json file
        string path = Application.persistentDataPath + "/Settings.json";
        if(File.Exists(path))
        {
            string jsonSaved = File.ReadAllText(path);
            OldSaveSettings savedParam = JsonUtility.FromJson<OldSaveSettings>(jsonSaved);
            savedParam.RestoreParam();
        }
        else
        {
            // default settings
            Settings.OverlayParam.DisplayMs = true;
            Settings.OverlayParam.DisplayFps = true;
            Settings.OverlayParam.DisplayCoords = true;
            
            // default keys in qwerty:
            Settings.KeyMap.Add("Forwards", KeyCode.W);
            Settings.KeyMap.Add("Backwards", KeyCode.S);
            Settings.KeyMap.Add("Left", KeyCode.A);
            Settings.KeyMap.Add("Right", KeyCode.D);
            Settings.KeyMap.Add("CamLeft", KeyCode.Q);
            Settings.KeyMap.Add("CamRight", KeyCode.E);
            Settings.KeyMap.Add("Kill", KeyCode.K);
            Settings.KeyMap.Add("TopView", KeyCode.T);
            Settings.KeyMap.Add("Inventory", KeyCode.Tab);
            Settings.KeyMap.Add("Respawn", KeyCode.R);
            
            // save the default settings
            SaveSettings();
        }

    }
    private void Update()
    {
        if (!_isReadingKey && Input.GetKeyDown(KeyCode.Escape)) // if the escape key is pressed but not while assigning a key
            if ((pauseMenu.activeSelf || !Settings.IsPaused) && !inventoryMenu.activeSelf) // toggle the pause menu except the inventory is shown
            {
                Settings.IsPaused = !Settings.IsPaused;
                pauseMenu.SetActive(Settings.IsPaused);
                if (Settings.IsPaused == false) SaveSettings(); // save the settings
            } else if (mainParamMenuButtons.activeSelf) ReturnToPauseMenu();
            else if (
                overlayMenu.activeSelf 
                || controlsMenu.activeSelf
                // otherMenu parameters
                // ...
            ) ReturnToMainParam(); // get back of one setting window
        
        if (!_isReadingKey) return; // if is not in key assign mode, the update is done

        if (Input.anyKey) // if a key is detected
            foreach(KeyCode key in Enum.GetValues(typeof(KeyCode))) // enum in all possible keys
                if (Input.GetKey(key))
                {
                    _isReadingKey = false;
                    pressKeyText.SetActive(_isReadingKey); //hide the text
                    
                    if(key == KeyCode.Escape) return; // cancel if escape key is pressed
                    
                    Settings.KeyMap[_function] = key; // map the key in the dictionary
                    _keyText.text = key.ToString();  // update the button text of the parameter 
                }
    }
}