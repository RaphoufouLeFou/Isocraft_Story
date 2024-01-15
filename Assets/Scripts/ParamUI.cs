using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public static class Parameters
{       
    public static bool IsPaused;
    public static Dictionary<string, KeyCode> KeyMap = new Dictionary<string, KeyCode>(); 
    public struct Overlay
    {
        public bool DisplayFps;
        public bool DisplayMspf;
        public bool DisplayCoordonates;
    }

    public static Overlay OverlayParam;
}
public class SaveParameters
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
        foreach (KeyValuePair<string,KeyCode> hKeyValuePair in Parameters.KeyMap)
        {
            KeyMapListStr.Add(hKeyValuePair.Key);
            KeyMapListKeys.Add(hKeyValuePair.Value);
        }
        IsPaused = Parameters.IsPaused;
        DisplayMspf = Parameters.OverlayParam.DisplayMspf;
        DisplayFps = Parameters.OverlayParam.DisplayFps;
        DisplayCoordonates = Parameters.OverlayParam.DisplayCoordonates;
        Debug.Log($"init, coo = {DisplayCoordonates}");
    }
    public void RestoreParm()
    {
        Parameters.KeyMap = new Dictionary<string, KeyCode>();
        for (int i = 0; i < KeyMapListStr.Count; i++)
        {
            Parameters.KeyMap.Add(KeyMapListStr[i], KeyMapListKeys[i]);
        }
        Parameters.IsPaused = IsPaused;
        Parameters.OverlayParam.DisplayMspf = DisplayMspf;
        Parameters.OverlayParam.DisplayFps = DisplayFps;
        Parameters.OverlayParam.DisplayCoordonates = DisplayCoordonates;
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
        LoadParameters();
    }

    public void ToggleFps()
    {
        Parameters.OverlayParam.DisplayFps = !Parameters.OverlayParam.DisplayFps;
    }
    public void ToggleMspf()
    {
        Parameters.OverlayParam.DisplayMspf = !Parameters.OverlayParam.DisplayMspf;
    }
    public void ToggleCoordonates()
    {
        Parameters.OverlayParam.DisplayCoordonates = !Parameters.OverlayParam.DisplayCoordonates;
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
        _function = function.name;      //For optimization, the function name is the name of the object in the UI
        _keyText = function.transform.GetChild(1).GetComponentInChildren<TMP_Text>(); //get the text of the button
    }
   
    
    public void ButtonResumeClick()
    {
        Parameters.IsPaused = !Parameters.IsPaused;
        pauseMenu.SetActive(Parameters.IsPaused);
        SaveParameters();
    }

    private void SaveParameters()
    {
        SaveParameters saveParameters = new SaveParameters();
        saveParameters.InitParm();
        string jsonSave = JsonUtility.ToJson(saveParameters, true);
        string path = Application.persistentDataPath + "/Settings.json";
        if(File.Exists(path)) File.Delete(path);
        File.WriteAllText(path, jsonSave);
    }

    private void LoadParameters()
    {

        string path = Application.persistentDataPath + "/Settings.json";
        if(File.Exists(path))
        {
            string jsonSaved = File.ReadAllText(path);
            SaveParameters savedParam = JsonUtility.FromJson<SaveParameters>(jsonSaved);
            savedParam.RestoreParm();
        }
        else
        {

            //defaults parameters

            Parameters.OverlayParam.DisplayMspf = true;
            Parameters.OverlayParam.DisplayFps = true;
            Parameters.OverlayParam.DisplayCoordonates = true;
            
            //defaults keys in qwerty:
            Parameters.KeyMap.Add("Forward", KeyCode.W);
            Parameters.KeyMap.Add("Backward", KeyCode.S);
            Parameters.KeyMap.Add("Left", KeyCode.A);
            Parameters.KeyMap.Add("Right", KeyCode.D);
            Parameters.KeyMap.Add("CamLeft", KeyCode.Q);
            Parameters.KeyMap.Add("CamRight", KeyCode.E);
            Parameters.KeyMap.Add("Kill", KeyCode.K);
            Parameters.KeyMap.Add("TopView", KeyCode.T);
            Parameters.KeyMap.Add("Spawn", KeyCode.R);

            SaveParameters();
        }

    }
    private void Update()
    {
        if (!_isReadingKey && Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseMenu.activeSelf || !Parameters.IsPaused)
            {
                Parameters.IsPaused = !Parameters.IsPaused;
                pauseMenu.SetActive(Parameters.IsPaused);
                if (Parameters.IsPaused == false) SaveParameters();
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
                    
                    Parameters.KeyMap[_function] = kcode;
                    _keyText.text = kcode.ToString();
                    Debug.Log(_function + " is now " +  kcode);
                }
            }
        }
    }
}