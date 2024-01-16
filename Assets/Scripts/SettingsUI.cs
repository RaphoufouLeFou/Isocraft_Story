using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


//setting class
public static class Settings
{       
    public static bool IsPaused;    //General setting that represent the paused state of the game
    public static Dictionary<string, KeyCode> KeyMap = new();  //dictionary with the input name as key and keycode as value, use for custom input system.
    public struct Overlay   // overlay infos struct
    {
        public bool DisplayFps; //frame per second
        public bool DisplayMspf;//ms per frame
        public bool DisplayCoordinates; //player coordonates
    }

    public static Overlay OverlayParam; // overlay infos to be displayed
}

//setting in the format to be understand by the dumb unity json converter
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
        //Convert the dictionary to two lists for the json
        KeyMapListStr = new List<string>();
        KeyMapListKeys = new List<KeyCode>();
        foreach (KeyValuePair<string,KeyCode> hKeyValuePair in Settings.KeyMap)
        {
            KeyMapListStr.Add(hKeyValuePair.Key);
            KeyMapListKeys.Add(hKeyValuePair.Value);
        }
        //Set the values to be saved
        IsPaused = Settings.IsPaused;
        DisplayMspf = Settings.OverlayParam.DisplayMspf;
        DisplayFps = Settings.OverlayParam.DisplayFps;
        DisplayCoordonates = Settings.OverlayParam.DisplayCoordinates;

    }
    public void RestoreParm()
    {
        //Convert the thw saved lists back to the dictionary
        Settings.KeyMap = new Dictionary<string, KeyCode>();
        for (int i = 0; i < KeyMapListStr.Count; i++)
        {
            Settings.KeyMap.Add(KeyMapListStr[i], KeyMapListKeys[i]);
        }
        
        //restore the saved values
        Settings.IsPaused = IsPaused;
        Settings.OverlayParam.DisplayMspf = DisplayMspf;
        Settings.OverlayParam.DisplayFps = DisplayFps;
        Settings.OverlayParam.DisplayCoordinates = DisplayCoordonates;
    }
}

public class SettingsUI : MonoBehaviour
{
    //menus gameobjects
    public GameObject mainParamMenu;
    public GameObject mainParamMenuButtons;
    public GameObject overlayMenu;
    public GameObject controlsMenu;
    public GameObject inventoryMenu;
    public GameObject pauseMenu;
    public GameObject pressKeyText;

    //variables when assigning a key
    private bool _isReadingKey; 
    private string _function;
    private TMP_Text _keyText;
    
    void Start()
    {
        // set all settings to hided
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
        // toggle the fps setting from the UI
        Settings.OverlayParam.DisplayFps = !Settings.OverlayParam.DisplayFps;
    }
    public void ToggleMspf()
    {
        // toggle the Mspf setting from the UI
        Settings.OverlayParam.DisplayMspf = !Settings.OverlayParam.DisplayMspf;
    }
    public void ToggleCoordinates()
    {
        // toggle the Coordinates setting from the UI
        Settings.OverlayParam.DisplayCoordinates = !Settings.OverlayParam.DisplayCoordinates;
    }
    public void EnterOverlayParam()
    {
        overlayMenu.SetActive(true);            //show the overlay menu buttons
        mainParamMenuButtons.SetActive(false);  //hide the main settings buttons
    }
    
    public void EnterControlsParam()
    {
        controlsMenu.SetActive(true);           //show the controls menu buttons
        mainParamMenuButtons.SetActive(false);  //hide the main settings buttons
    }


    public void ReturnToMainParam()
    {
        //Go back button in sub-settings
        mainParamMenu.SetActive(true);
        //all sub-settings
        overlayMenu.SetActive(false);
        controlsMenu.SetActive(false);
        //also called by th settings button in the pause menu, so we need to hide it now
        pauseMenu.SetActive(false);
        //show the settings buttons
        mainParamMenuButtons.SetActive(true);
    }

    public void ReturnToPauseMenu()
    {
        //Go back button to main settings
        mainParamMenu.SetActive(false);
        pauseMenu.SetActive(true);
        mainParamMenuButtons.SetActive(false);
    }

    public void AssignKey(GameObject function)
    {
        _isReadingKey = true;
        pressKeyText.SetActive(_isReadingKey);  //show the "press a key" text
        _function = function.name; // for optimization, the function name is the name of the input in the UI
        _keyText = function.transform.GetChild(1).GetComponentInChildren<TMP_Text>(); // get the press Key Text to hide it later
    }
   
    
    public void ButtonResumeClick()
    {
        //hide the pause menu
        Settings.IsPaused = !Settings.IsPaused;
        pauseMenu.SetActive(Settings.IsPaused);
        SaveSettings();
    }

    private void SaveSettings()
    {
        //save the settings into a json file in the AppData folder
        SaveSettings saveSettings = new SaveSettings();
        saveSettings.InitParm();
        string jsonSave = JsonUtility.ToJson(saveSettings, true);
        string path = Application.persistentDataPath + "/Settings.json";
        if(File.Exists(path)) File.Delete(path);
        File.WriteAllText(path, jsonSave);
    }

    private void LoadSettings()
    {
        //read the settings from the json file
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
            
            //defaults keys in qwerty:

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
            
            //save the default settings
            SaveSettings();
        }

    }
    private void Update()
    {
        if (!_isReadingKey && Input.GetKeyDown(KeyCode.Escape)) //if the escape key is pressed but not while assigning a key
        {
            if ((pauseMenu.activeSelf || !Settings.IsPaused) && !inventoryMenu.activeSelf) //toggle the pause menu except the inventory is shown
            {
                Settings.IsPaused = !Settings.IsPaused;
                pauseMenu.SetActive(Settings.IsPaused);
                if (Settings.IsPaused == false) SaveSettings(); //save the settings
            }else if (mainParamMenuButtons.activeSelf) ReturnToPauseMenu();
            else if (
                overlayMenu.activeSelf 
                || controlsMenu.activeSelf
                // otherMenu parameters
                // ...
            ) ReturnToMainParam();  //get back of one setting window
        }
        
        if (!_isReadingKey) return; //if is not in key assignation, the update is done

        if (Input.anyKey)   // if a key is detected
        {
            foreach(KeyCode kcode in Enum.GetValues(typeof(KeyCode))) // enum in all possible key (no choice) 
            {
                if (Input.GetKey(kcode))
                {   //kcode is now the key pressed
                    _isReadingKey = false;
                    pressKeyText.SetActive(_isReadingKey);  //hide the text
                    
                    if(kcode == KeyCode.Escape) return;     //cancel if escape key is pressed
                    
                    Settings.KeyMap[_function] = kcode; //map the key in the dictionary
                    _keyText.text = kcode.ToString();   //update the button text of the parameter 
                }
            }
        }
    }
}