using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public static class Settings
{       
    public static bool IsPaused;
    public static Dictionary<string, KeyCode> KeyMap = new(); 
    public struct Overlay
    {
        public bool DisplayFps;
        public bool DisplayMspf;
        public bool DisplayCoordinates;
    }

    public static Overlay OverlayParam;
}
public class SaveSettings
{       
    public List<string> KeyMapListStr;
    public List<KeyCode> KeyMapListKeys;
    public bool IsPaused;
    public bool DisplayFps;
    public bool DisplayMspf;
    public bool DisplayCoordonates;
    
    public void InitParm()
    {
        
        KeyMapListStr = new List<string>();
        KeyMapListKeys = new List<KeyCode>();
        foreach (KeyValuePair<string,KeyCode> hKeyValuePair in Settings.KeyMap)
        {
            KeyMapListStr.Add(hKeyValuePair.Key);
            KeyMapListKeys.Add(hKeyValuePair.Value);
        }
        IsPaused = Settings.IsPaused;
        DisplayMspf = Settings.OverlayParam.DisplayMspf;
        DisplayFps = Settings.OverlayParam.DisplayFps;
        DisplayCoordonates = Settings.OverlayParam.DisplayCoordinates;
        Debug.Log($"init, coo = {DisplayCoordonates}");
    }
    public void RestoreParm()
    {
        Settings.KeyMap = new Dictionary<string, KeyCode>();
        for (int i = 0; i < KeyMapListStr.Count; i++)
        {
            Settings.KeyMap.Add(KeyMapListStr[i], KeyMapListKeys[i]);
        }
        Settings.IsPaused = IsPaused;
        Settings.OverlayParam.DisplayMspf = DisplayMspf;
        Settings.OverlayParam.DisplayFps = DisplayFps;
        Settings.OverlayParam.DisplayCoordinates = DisplayCoordonates;
    }
}

public class ParamUI : MonoBehaviour
{
    public GameObject mainParamMenu;
    public GameObject mainParamMenuButtons;
    public GameObject overlayMenu;
    public GameObject controlsMenu;
    public GameObject pauseMenu;
    public GameObject pressKeyText;

    private bool _isReadingKey;
    private string _function;
    private TMP_Text _keyText;
    void Start()
    {
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
    public void ToggleMspf()
    {
        Settings.OverlayParam.DisplayMspf = !Settings.OverlayParam.DisplayMspf;
    }
    public void ToggleCoordinates()
    {
        Settings.OverlayParam.DisplayCoordinates = !Settings.OverlayParam.DisplayCoordinates;
    }
    public void EnterOverlayParam()
    {
        overlayMenu.SetActive(true);
        mainParamMenuButtons.SetActive(false);
    }
    
    public void EnterControlsParam()
    {
        controlsMenu.SetActive(true);
        mainParamMenuButtons.SetActive(false);
    }


    public void ReturnToMainParam()
    {
        mainParamMenu.SetActive(true);
        overlayMenu.SetActive(false);
        controlsMenu.SetActive(false);
        pauseMenu.SetActive(false);
        mainParamMenuButtons.SetActive(true);
    }

    public void ReturnToPauseMenu()
    {
        mainParamMenu.SetActive(false);
        pauseMenu.SetActive(true);
        mainParamMenuButtons.SetActive(false);
    }

    public void AssignKey(GameObject function)
    {
        _isReadingKey = true;
        pressKeyText.SetActive(_isReadingKey);
        _function = function.name; // for optimization, the function name is the name of the object in the UI
        _keyText = function.transform.GetChild(1).GetComponentInChildren<TMP_Text>(); // get the text of the button
    }
   
    
    public void ButtonResumeClick()
    {
        Settings.IsPaused = !Settings.IsPaused;
        pauseMenu.SetActive(Settings.IsPaused);
        SaveSettings();
    }

    private void SaveSettings()
    {
        SaveSettings saveSettings = new SaveSettings();
        saveSettings.InitParm();
        string jsonSave = JsonUtility.ToJson(saveSettings, true);
        string path = Application.persistentDataPath + "/Settings.json";
        if(File.Exists(path)) File.Delete(path);
        File.WriteAllText(path, jsonSave);
    }

    private void LoadSettings()
    {

        string path = Application.persistentDataPath + "/Settings.json";
        if(File.Exists(path))
        {
            string jsonSaved = File.ReadAllText(path);
            SaveSettings savedParam = JsonUtility.FromJson<SaveSettings>(jsonSaved);
            savedParam.RestoreParm();
        }
        else
        {

            // default settings

            Settings.OverlayParam.DisplayMspf = true;
            Settings.OverlayParam.DisplayFps = true;
            Settings.OverlayParam.DisplayCoordinates = true;
            
            // default keys in qwerty:
            Settings.KeyMap.Add("Forwards", KeyCode.W);
            Settings.KeyMap.Add("Backwards", KeyCode.S);
            Settings.KeyMap.Add("Left", KeyCode.A);
            Settings.KeyMap.Add("Right", KeyCode.D);
            Settings.KeyMap.Add("CamLeft", KeyCode.Q);
            Settings.KeyMap.Add("CamRight", KeyCode.E);
            Settings.KeyMap.Add("Kill", KeyCode.K);
            Settings.KeyMap.Add("TopView", KeyCode.T);
            Settings.KeyMap.Add("Respawn", KeyCode.R);

            SaveSettings();
        }

    }
    private void Update()
    {
        if (!_isReadingKey && Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseMenu.activeSelf || !Settings.IsPaused)
            {
                Settings.IsPaused = !Settings.IsPaused;
                pauseMenu.SetActive(Settings.IsPaused);
                if (Settings.IsPaused == false) SaveSettings();
            }else if (mainParamMenuButtons.activeSelf) ReturnToPauseMenu();
            else if (
                overlayMenu.activeSelf 
                || controlsMenu.activeSelf
                // otherMenu windows
                // ...
            ) ReturnToMainParam();
        }
        
        if (!_isReadingKey) return;

        if (Input.anyKey)
        {
            foreach(KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKey(kcode))
                {
                    _isReadingKey = false;
                    pressKeyText.SetActive(_isReadingKey);
                    
                    if(kcode == KeyCode.Escape) return;     //cancel if escape key is pressed
                    
                    Settings.KeyMap[_function] = kcode;
                    _keyText.text = kcode.ToString();
                    Debug.Log(_function + " is now " +  kcode);
                }
            }
        }
    }
}